using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance bentuk data untuk <c>BE-RWI-040</c>, <c>BE-RWI-041</c>, dan
    /// <c>BE-RWI-042</c>.
    /// </summary>
    /// <remarks>
    /// Uji ini membaca model EF Core yang sama dengan yang dipakai aplikasi, sehingga kolom,
    /// nullability, index, dan nama tabel yang diperiksa adalah bentuk yang benar-benar akan
    /// dibuat migration — bukan salinan yang ditulis ulang di dalam uji.
    /// </remarks>
    public class InpatientClinicalSchemaTests
    {
        private static IEntityType Entity<T>(ApplicationDbContext c) =>
            c.Model.FindEntityType(typeof(T))!;

        private static IProperty Kolom<T>(ApplicationDbContext c, string nama) =>
            Entity<T>(c).FindProperty(nama)!;

        // =====================================================================
        // BE-RWI-040 - tiga belas kolom pada empat tabel klinis
        // =====================================================================

        /// <summary>
        /// `BE-RWI-040 AC 1` — ketiga belas kolom terbentuk, seluruhnya nullable kecuali dua
        /// kolom berjenis enum yang punya nilai bawaan.
        /// </summary>
        /// <remarks>
        /// Pembagiannya: catatan dokter tiga, catatan terpadu lima, tindakan tiga, dan
        /// pengkajian dua. Dua kolom pengkajian dipakai bersama sub-modul keperawatan lewat
        /// <c>INT-DOK-09</c> dan dibuat di sini karena saat task berjalan keduanya belum ada.
        /// </remarks>
        [Fact]
        public void TigaBelasKolomKonteks_TerbentukSesuaiKamusData()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var nullable = new (Type Tabel, string Kolom)[]
            {
                (typeof(TrxDoctorConsultation), "InpEpisodeId"),
                (typeof(TrxDoctorConsultation), "ClinicalDateTime"),
                (typeof(TrxDoctorConsultation), "PhysicianVisitId"),
                (typeof(TrxPatientIntegratedProgressNote), "InpEpisodeId"),
                (typeof(TrxPatientIntegratedProgressNote), "VerifiedAt"),
                (typeof(TrxPatientIntegratedProgressNote), "VerifiedByUserId"),
                (typeof(TrxPatientIntegratedProgressNote), "VerificationDueAt"),
                (typeof(TrxPatientProcedure), "InpEpisodeId"),
                (typeof(TrxPatientProcedure), "PhysicianVisitId"),
                (typeof(TrxPatientProcedure), "IdempotencyKey"),
                (typeof(TrxPatientAssessment), "InpEpisodeId")
            };

            foreach (var (tabel, kolom) in nullable)
            {
                var properti = context.Model.FindEntityType(tabel)!.FindProperty(kolom);

                Assert.True(properti != null, $"{tabel.Name}.{kolom} tidak ditemukan.");
                Assert.True(properti!.IsNullable, $"{tabel.Name}.{kolom} seharusnya nullable.");
            }

            // Dua kolom yang wajib, keduanya bernilai bawaan supaya baris lama tidak disentuh.
            var verifikasi = Kolom<TrxPatientIntegratedProgressNote>(context, "VerificationStatus");
            Assert.False(verifikasi.IsNullable);
            Assert.Equal(CpptVerificationStatus.NotRequired, verifikasi.GetDefaultValue());

            var jenisKajian = Kolom<TrxPatientAssessment>(context, "AssessmentType");
            Assert.False(jenisKajian.IsNullable);
            Assert.Equal(PatientAssessmentType.Initial, jenisKajian.GetDefaultValue());

            Assert.Equal(13, nullable.Length + 2);
        }

        /// <summary>`BE-RWI-040 AC 2` — index lini masa per perawatan terbentuk.</summary>
        [Fact]
        public void IndexLiniMasaPerPerawatan_Terbentuk()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            AssertPunyaIndex<TrxDoctorConsultation>(context, "InpEpisodeId", "ClinicalDateTime");
            AssertPunyaIndex<TrxPatientIntegratedProgressNote>(context, "InpEpisodeId", "NoteDateTime");
            AssertPunyaIndex<TrxPatientProcedure>(context, "InpEpisodeId", "PerformedAt");
            AssertPunyaIndex<CliPhysicianVisit>(context, "InpEpisodeId", "VisitDateTime");
        }

        /// <summary>
        /// `BE-RWI-040 AC 3` — enum jenis pengkajian memuat empat nilai keperawatan dan dua nilai
        /// kajian medis, dan nilai lamanya tidak bergeser.
        /// </summary>
        [Fact]
        public void EnumJenisPengkajian_MemuatDuaNilaiKajianMedis()
        {
            Assert.Equal(6, Enum.GetValues<PatientAssessmentType>().Length);

            // Nilai keperawatan menempati angka aslinya. Menggesernya berarti menulis ulang arti
            // baris yang sudah tersimpan.
            Assert.Equal(0, (int)PatientAssessmentType.Initial);
            Assert.Equal(1, (int)PatientAssessmentType.Reassessment);
            Assert.Equal(2, (int)PatientAssessmentType.DailyReassessment);
            Assert.Equal(3, (int)PatientAssessmentType.DischargePlanning);

            Assert.Equal(4, (int)PatientAssessmentType.MedicalInitial);
            Assert.Equal(5, (int)PatientAssessmentType.MedicalReassessment);
        }

        /// <summary>Enum keadaan verifikasi CPPT memuat empat nilai, bawaan `NotRequired`.</summary>
        [Fact]
        public void EnumKeadaanVerifikasi_MemuatEmpatNilai()
        {
            Assert.Equal(4, Enum.GetValues<CpptVerificationStatus>().Length);
            Assert.Equal(0, (int)CpptVerificationStatus.NotRequired);
            Assert.Equal(1, (int)CpptVerificationStatus.Pending);
        }

        /// <summary>
        /// Kunci permintaan tindakan dijaga unique parsial database, bukan hanya oleh service.
        /// </summary>
        [Fact]
        public void KunciPermintaanTindakan_DijagaUniqueParsial()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var index = Entity<TrxPatientProcedure>(context).GetIndexes()
                .Single(x => x.Properties.Count == 1 &&
                             x.Properties[0].Name == "IdempotencyKey");

            Assert.True(index.IsUnique);
            Assert.Contains("IsDelete", index.GetFilter());
        }

        // =====================================================================
        // BE-RWI-041 - tabel kejadian visite
        // =====================================================================

        /// <summary>
        /// `BE-RWI-041 AC 1, 2, 3, 4` — nama tabel, unique penuh pada kunci permintaan, kedua
        /// index waktu, dan ketiadaan unique atas pasangan perawatan-dokter-tanggal.
        /// </summary>
        [Fact]
        public void TabelVisite_BentuknyaSesuaiKamusData()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var entity = Entity<CliPhysicianVisit>(context);

            // AC 1 - bukan berawalan Trx.
            Assert.Equal("CliPhysicianVisit", entity.GetTableName());
            Assert.Equal("public", entity.GetSchema());
            Assert.StartsWith("Cli", nameof(CliPhysicianVisit));
            Assert.False(nameof(CliPhysicianVisit).StartsWith("Trx", StringComparison.Ordinal));

            // AC 2 - kunci permintaan wajib terisi dan unique penuh, tanpa filter.
            var kunci = Kolom<CliPhysicianVisit>(context, "IdempotencyKey");
            Assert.False(kunci.IsNullable);

            var indexKunci = entity.GetIndexes()
                .Single(x => x.Properties.Count == 1 && x.Properties[0].Name == "IdempotencyKey");
            Assert.True(indexKunci.IsUnique);
            Assert.Null(indexKunci.GetFilter());

            // AC 3 - index perawatan-waktu dan dokter-waktu.
            AssertPunyaIndex<CliPhysicianVisit>(context, "InpEpisodeId", "VisitDateTime");
            AssertPunyaIndex<CliPhysicianVisit>(context, "DoctorId", "VisitDateTime");

            // AC 4 - tidak ada unique atas pasangan perawatan, dokter, dan tanggal.
            Assert.DoesNotContain(entity.GetIndexes(), x =>
                x.IsUnique &&
                x.Properties.Any(p => p.Name == "DoctorId") &&
                x.Properties.Any(p => p.Name == "InpEpisodeId"));

            // Nomor bisnis unik.
            var indexNomor = entity.GetIndexes()
                .Single(x => x.Properties.Count == 1 && x.Properties[0].Name == "PhysicianVisitNumber");
            Assert.True(indexNomor.IsUnique);
        }

        /// <summary>
        /// `BE-RWI-041 AC 5` — nomor bisnis dialokasikan service, tidak dibentuk dari hitungan
        /// baris.
        /// </summary>
        /// <remarks>
        /// Dua nomor yang dibentuk pada detik yang sama tetap berbeda. Alokasi berbasis
        /// <c>Count + 1</c> atau <c>Max + 1</c> akan menghasilkan dua nomor kembar di sini —
        /// persis kegagalan yang dilarang <c>QBE-CODE-003</c>.
        /// </remarks>
        [Fact]
        public void NomorVisite_TidakDibentukDariHitunganBaris()
        {
            var service = new PhysicianVisitNumberService();
            var saatYangSama = new DateTime(2026, 9, 3, 7, 40, 12, DateTimeKind.Utc);

            var nomor = Enumerable.Range(0, 200)
                .Select(_ => service.Generate(null, saatYangSama))
                .ToList();

            Assert.Equal(nomor.Count, nomor.Distinct().Count());
            Assert.All(nomor, x => Assert.StartsWith("VST-260903074012-", x));
            Assert.All(nomor, x => Assert.True(x.Length <= 30, $"Nomor {x} melebihi 30 karakter."));
        }

        /// <summary>
        /// Kiriman ulang berkunci sama mengembalikan kejadian yang sama, bukan kejadian kedua.
        /// </summary>
        [Fact]
        public async Task VisiteBerkunciSama_TidakMelahirkanKejadianKedua()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var aktor = RekamMedisTestData.BuatPengguna(context, "perekam");

            var service = new PhysicianVisitService(context, new PhysicianVisitNumberService());

            RecordPhysicianVisitCommand Perintah() => new()
            {
                EncounterId = k.EncounterId,
                InpEpisodeId = k.EpisodeId,
                PatientId = k.PatientId,
                DoctorId = k.DoctorMasterId,
                VisitDateTime = DateTime.UtcNow.AddHours(-1),
                IdempotencyKey = "kunci-uji-01"
            };

            var pertama = await service.RecordAsync(Perintah(), aktor.Id);
            var kedua = await service.RecordAsync(Perintah(), aktor.Id);

            Assert.True(pertama.IsSuccess);
            Assert.True(kedua.IsSuccess);
            Assert.False(pertama.IsReplay);
            Assert.True(kedua.IsReplay);
            Assert.Equal(pertama.Visit!.Id, kedua.Visit!.Id);
            Assert.Equal(201, pertama.StatusCode);
            Assert.Equal(200, kedua.StatusCode);

            Assert.Equal(1, await context.CliPhysicianVisits.CountAsync());
        }

        /// <summary>
        /// `RWI-DEC-085` — dua visite nyata pada tanggal yang sama menghasilkan dua baris dan
        /// hitungan dua.
        /// </summary>
        [Fact]
        public async Task DuaVisitePadaTanggalSama_MenghasilkanDuaBaris()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var aktor = RekamMedisTestData.BuatPengguna(context, "perekam");

            var service = new PhysicianVisitService(context, new PhysicianVisitNumberService());
            var hariIni = DateTime.UtcNow.Date.AddHours(7);

            var pagi = await service.RecordAsync(new RecordPhysicianVisitCommand
            {
                EncounterId = k.EncounterId,
                InpEpisodeId = k.EpisodeId,
                PatientId = k.PatientId,
                DoctorId = k.DoctorMasterId,
                VisitDateTime = hariIni,
                IdempotencyKey = "kunci-pagi"
            }, aktor.Id);

            var sore = await service.RecordAsync(new RecordPhysicianVisitCommand
            {
                EncounterId = k.EncounterId,
                InpEpisodeId = k.EpisodeId,
                PatientId = k.PatientId,
                DoctorId = k.DoctorMasterId,
                VisitDateTime = hariIni.AddHours(9),
                IdempotencyKey = "kunci-sore"
            }, aktor.Id);

            Assert.True(pagi.IsSuccess);
            Assert.True(sore.IsSuccess);
            Assert.NotEqual(pagi.Visit!.Id, sore.Visit!.Id);
            Assert.Equal(2, await service.CountRecordedByEpisodeAsync(k.EpisodeId));
        }

        /// <summary>
        /// Kejadian yang dibatalkan tetap tersimpan, tetap terbaca pada riwayat, dan tidak ikut
        /// dihitung — <c>INV-DOK-08</c>.
        /// </summary>
        [Fact]
        public async Task VisiteYangDibatalkan_TetapTersimpanDanTidakDihitung()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var aktor = RekamMedisTestData.BuatPengguna(context, "perekam");

            var service = new PhysicianVisitService(context, new PhysicianVisitNumberService());

            var dicatat = await service.RecordAsync(new RecordPhysicianVisitCommand
            {
                EncounterId = k.EncounterId,
                InpEpisodeId = k.EpisodeId,
                PatientId = k.PatientId,
                DoctorId = k.DoctorMasterId,
                VisitDateTime = DateTime.UtcNow,
                IdempotencyKey = "kunci-batal"
            }, aktor.Id);

            var tanpaAlasan = await service.CancelAsync(dicatat.Visit!.Id, "   ", aktor.Id);
            Assert.False(tanpaAlasan.IsSuccess);
            Assert.Equal(400, tanpaAlasan.StatusCode);

            var dibatalkan = await service.CancelAsync(dicatat.Visit.Id, "Salah jam", aktor.Id);
            Assert.True(dibatalkan.IsSuccess);

            var ulang = await service.CancelAsync(dicatat.Visit.Id, "Salah jam", aktor.Id);
            Assert.False(ulang.IsSuccess);
            Assert.Equal(409, ulang.StatusCode);

            var riwayat = await service.GetByEpisodeAsync(k.EpisodeId);

            Assert.Single(riwayat);
            Assert.Equal(PhysicianVisitStatus.Cancelled, riwayat[0].VisitStatus);
            Assert.Equal("Salah jam", riwayat[0].CancelReason);
            Assert.Equal(0, await service.CountRecordedByEpisodeAsync(k.EpisodeId));
        }

        // =====================================================================
        // BE-RWI-042 - konteks pada resep dan pesanan penunjang
        // =====================================================================

        /// <summary>
        /// `BE-RWI-042 AC 1, 2` — ketiga kolom konteks terbentuk nullable, dan jenis resep memuat
        /// rutin, harian, serta obat pulang dengan bawaan rutin.
        /// </summary>
        [Fact]
        public void KolomKonteksResepDanPesananPenunjang_Terbentuk()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            Assert.True(Kolom<TrxPrescription>(context, "InpEpisodeId").IsNullable);
            Assert.True(Kolom<LabOrder>(context, "InpEpisodeId").IsNullable);
            Assert.True(Kolom<RadOrder>(context, "InpEpisodeId").IsNullable);

            var jenis = Kolom<TrxPrescription>(context, "PrescriptionOrderType");
            Assert.False(jenis.IsNullable);
            Assert.Equal(PrescriptionOrderType.Routine, jenis.GetDefaultValue());

            Assert.Equal(3, Enum.GetValues<PrescriptionOrderType>().Length);
            Assert.Equal(0, (int)PrescriptionOrderType.Routine);
            Assert.Equal(1, (int)PrescriptionOrderType.Daily);
            Assert.Equal(2, (int)PrescriptionOrderType.Discharge);
        }

        /// <summary>
        /// `BE-RWI-043` — batas satu resep aktif per catatan dilepas hanya bagi resep yang
        /// menempel pada perawatan rawat inap.
        /// </summary>
        [Fact]
        public void UniqueResepAktif_HanyaBerlakuTanpaKonteksPerawatan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var index = Entity<TrxPrescription>(context).GetIndexes()
                .Single(x => x.Properties.Count == 1 && x.Properties[0].Name == "ConsultationId");

            Assert.True(index.IsUnique);
            Assert.Contains("InpEpisodeId", index.GetFilter());
        }

        /// <summary>
        /// `BE-RWI-043` — batas satu catatan per kunjungan dilepas hanya bagi catatan yang
        /// menempel pada perawatan rawat inap.
        /// </summary>
        [Fact]
        public void UniqueCatatanPerKunjungan_HanyaBerlakuTanpaKonteksPerawatan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var index = Entity<TrxDoctorConsultation>(context).GetIndexes()
                .Single(x => x.Properties.Count == 1 && x.Properties[0].Name == "EncounterId");

            Assert.True(index.IsUnique);
            Assert.Contains("InpEpisodeId", index.GetFilter());
        }

        private static void AssertPunyaIndex<T>(ApplicationDbContext context, params string[] kolom)
        {
            var ada = Entity<T>(context).GetIndexes().Any(x =>
                x.Properties.Count == kolom.Length &&
                x.Properties.Select(p => p.Name).SequenceEqual(kolom));

            Assert.True(ada, $"{typeof(T).Name} tidak memiliki index ({string.Join(", ", kolom)}).");
        }
    }
}
