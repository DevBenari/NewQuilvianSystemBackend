using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.HealthServices.MasterData
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-055</c> — batas waktu pengkajian dibaca dari master,
    /// bukan ditanam di kode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Yang dijaga berkas ini bukan sekadar tabelnya ada, melainkan <b>tiga keadaan master</b>
    /// yang menentukan apakah pemantauan kepatuhan berguna atau menyesatkan: master kosong,
    /// master terisi, dan kebijakan yang berubah di tengah perawatan.
    /// </para>
    /// <para>
    /// Seluruh data di bawah adalah data karangan.
    /// </para>
    /// </remarks>
    public class ClinicalAssessmentPolicyMasterTests
    {
        private static ClinicalAssessmentPolicyController BuatController(
            ApplicationDbContext c, Guid actorUserId) =>
            (ClinicalAssessmentPolicyController)new ClinicalAssessmentPolicyController(
                new ClinicalAssessmentPolicyService(c),
                ControllerTestHarness.BuatLoggerService(actorUserId))
                .DenganPengguna(actorUserId);

        private static CreateClinicalAssessmentPolicyRequest Permintaan(
            string kode = "KEP-AWAL-RI",
            int menit = 1440,
            ServiceUnitType? jenisPelayanan = null,
            DateTime? berlakuMulai = null,
            DateTime? berlakuSampai = null) => new()
            {
                PolicyCode = kode,
                PolicyName = "Batas waktu pengkajian awal rawat inap",
                AssessmentType = PatientAssessmentType.Initial,
                ServiceUnitType = jenisPelayanan,
                DueWithinMinutes = menit,
                EffectiveFrom = berlakuMulai ?? DateTime.UtcNow.AddDays(-30),
                EffectiveTo = berlakuSampai,
                IsActive = true
            };

        // =====================================================================
        // Kriteria 1 — master menyimpan batas waktu per jenis pengkajian per pelayanan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-055 AC 1` — master menyimpan batas waktu per jenis pengkajian dan per jenis
        /// pelayanan, beserta kode unik dan periode berlakunya.
        /// </summary>
        [Fact]
        public async Task MasterMenyimpanBatasWaktuPerJenisPengkajianDanPelayanan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var admin = RekamMedisTestData.BuatPengguna(context, "governance");

            var hasil = await BuatController(context, admin.Id)
                .Create(Permintaan(menit: 480, jenisPelayanan: ServiceUnitType.Inpatient));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();
            var tersimpan = await pembaca.Set<MstClinicalAssessmentPolicy>().SingleAsync();

            Assert.Equal("KEP-AWAL-RI", tersimpan.PolicyCode);
            Assert.Equal(PatientAssessmentType.Initial, tersimpan.AssessmentType);
            Assert.Equal(ServiceUnitType.Inpatient, tersimpan.ServiceUnitType);
            Assert.Equal(480, tersimpan.DueWithinMinutes);
            Assert.True(tersimpan.IsActive);
            Assert.Null(tersimpan.EffectiveTo);
        }

        /// <summary>Kode kebijakan kembar ditolak <c>409</c>, bukan menimpa yang sudah ada.</summary>
        [Fact]
        public async Task KodeKebijakanKembar_Ditolak409()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var admin = RekamMedisTestData.BuatPengguna(context, "governance");
            var controller = BuatController(context, admin.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(await controller.Create(Permintaan())));

            var kedua = await controller.Create(Permintaan());

            Assert.Equal(409, ControllerTestHarness.KodeStatus(kedua));
            Assert.Equal(1, await context.Set<MstClinicalAssessmentPolicy>().CountAsync());
        }

        /// <summary>Batas waktu nol atau negatif ditolak <c>400</c>.</summary>
        [Fact]
        public async Task BatasWaktuNol_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var admin = RekamMedisTestData.BuatPengguna(context, "governance");

            var hasil = await BuatController(context, admin.Id).Create(Permintaan(menit: 0));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(0, await context.Set<MstClinicalAssessmentPolicy>().CountAsync());
        }

        // =====================================================================
        // Kriteria 2 — kebijakan berversi
        // =====================================================================

        /// <summary>
        /// `BE-RWI-055 AC 2` — pemilihan kebijakan memakai keadaan pada saat yang ditanyakan,
        /// bukan keadaan hari ini.
        /// </summary>
        /// <remarks>
        /// <b>Contoh berangka yang diuji.</b> Kebijakan lama 1440 menit berlaku 1–20 September;
        /// kebijakan baru 480 menit berlaku sejak 20 September. Pertanyaan "berapa batas waktu
        /// pada 10 September" dijawab 1440, dan pertanyaan "berapa batas waktu sekarang" dijawab
        /// 480. Tanpa pembedaan ini, pengkajian yang dulu tepat waktu berubah menjadi terlambat
        /// hanya karena angkanya diperbarui — <c>AC-CAP012-04</c>.
        /// </remarks>
        [Fact]
        public async Task KebijakanBerversi_DipilihMenurutSaatYangDitanyakan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var admin = RekamMedisTestData.BuatPengguna(context, "governance");
            var controller = BuatController(context, admin.Id);

            var awalBulan = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
            var pertengahan = new DateTime(2026, 9, 20, 0, 0, 0, DateTimeKind.Utc);

            await controller.Create(Permintaan(
                kode: "KEP-LAMA", menit: 1440,
                berlakuMulai: awalBulan, berlakuSampai: pertengahan));

            await controller.Create(Permintaan(
                kode: "KEP-BARU", menit: 480,
                berlakuMulai: pertengahan));

            var service = new ClinicalAssessmentPolicyService(context);

            var sepuluhSeptember = new DateTime(2026, 9, 10, 8, 0, 0, DateTimeKind.Utc);
            var duaPuluhLimaSeptember = new DateTime(2026, 9, 25, 8, 0, 0, DateTimeKind.Utc);

            var lama = await service.ResolveEffectiveAsync(
                PatientAssessmentType.Initial, ServiceUnitType.Inpatient, sepuluhSeptember);

            var baru = await service.ResolveEffectiveAsync(
                PatientAssessmentType.Initial, ServiceUnitType.Inpatient, duaPuluhLimaSeptember);

            Assert.NotNull(lama);
            Assert.NotNull(baru);
            Assert.Equal("KEP-LAMA", lama!.PolicyCode);
            Assert.Equal(1440, lama.DueWithinMinutes);
            Assert.Equal("KEP-BARU", baru!.PolicyCode);
            Assert.Equal(480, baru.DueWithinMinutes);

            // Tenggat dihitung dari saat pengkajian dibuat, memakai kebijakan saat itu.
            var tenggatLama = await service.CalculateDueAsync(
                PatientAssessmentType.Initial, ServiceUnitType.Inpatient, sepuluhSeptember);

            Assert.Equal(sepuluhSeptember.AddMinutes(1440), tenggatLama.DueAt);
            Assert.Equal(lama.Id, tenggatLama.PolicyId);
        }

        /// <summary>
        /// Kebijakan yang menyebut jenis pelayanan mengalahkan kebijakan umum, sehingga rumah
        /// sakit dapat menetapkan satu angka umum lalu mengecualikan unit tertentu.
        /// </summary>
        [Fact]
        public async Task KebijakanKhusus_MengalahkanKebijakanUmum()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var admin = RekamMedisTestData.BuatPengguna(context, "governance");
            var controller = BuatController(context, admin.Id);

            await controller.Create(Permintaan(kode: "KEP-UMUM", menit: 1440));
            await controller.Create(Permintaan(
                kode: "KEP-KHUSUS", menit: 480, jenisPelayanan: ServiceUnitType.Inpatient));

            var service = new ClinicalAssessmentPolicyService(context);

            var diRawatInap = await service.ResolveEffectiveAsync(
                PatientAssessmentType.Initial, ServiceUnitType.Inpatient, DateTime.UtcNow);

            var diPoliklinik = await service.ResolveEffectiveAsync(
                PatientAssessmentType.Initial, ServiceUnitType.Outpatient, DateTime.UtcNow);

            Assert.Equal("KEP-KHUSUS", diRawatInap!.PolicyCode);
            Assert.Equal("KEP-UMUM", diPoliklinik!.PolicyCode);
        }

        // =====================================================================
        // Kriteria 3 — master kosong tidak menahan apa pun
        // =====================================================================

        /// <summary>
        /// `BE-RWI-055 AC 3` — master kosong tidak menghasilkan satu pun tenggat, dan rekapnya
        /// menyatakan keadaan itu apa adanya.
        /// </summary>
        [Fact]
        public async Task MasterKosong_TidakMenghasilkanTenggatDanDinyatakanApaAdanya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var service = new ClinicalAssessmentPolicyService(context);

            Assert.True(await service.IsMasterEmptyAsync());

            var tenggat = await service.CalculateDueAsync(
                PatientAssessmentType.Initial, ServiceUnitType.Inpatient, DateTime.UtcNow);

            Assert.Null(tenggat.DueAt);
            Assert.Null(tenggat.PolicyId);

            var admin = RekamMedisTestData.BuatPengguna(context, "governance");
            var rekap = await BuatController(context, admin.Id).GetSummary();

            Assert.Equal(200, ControllerTestHarness.KodeStatus(rekap));
            Assert.Contains("belum ditetapkan", ControllerTestHarness.Pesan(rekap)!);
        }

        /// <summary>
        /// Kebijakan yang dinonaktifkan berhenti dipakai ke depan, tetapi <b>tidak</b> menyentuh
        /// satu pun pengkajian yang sudah memakainya.
        /// </summary>
        [Fact]
        public async Task KebijakanDinonaktifkan_TidakMengubahPengkajianYangSudahAda()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var admin = RekamMedisTestData.BuatPengguna(context, "governance");
            var controller = BuatController(context, admin.Id);

            await controller.Create(Permintaan(menit: 1440, jenisPelayanan: ServiceUnitType.Inpatient));

            var kebijakan = await context.Set<MstClinicalAssessmentPolicy>().SingleAsync();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var pengkajian = new TrxPatientAssessment
            {
                AssessmentNumber = $"ASM-{Guid.NewGuid():N}"[..20],
                EncounterId = k.EncounterId,
                PatientId = k.PatientId,
                ServiceUnitId = k.ServiceUnitId,
                InpEpisodeId = k.EpisodeId,
                AssessmentType = PatientAssessmentType.Initial,
                DueAt = DateTime.UtcNow.AddHours(24),
                PolicyId = kebijakan.Id
            };
            context.Set<TrxPatientAssessment>().Add(pengkajian);
            await context.SaveChangesAsync();

            var dinonaktifkan = await controller.UpdateStatus(
                kebijakan.Id, new UpdateClinicalAssessmentPolicyStatusRequest { IsActive = false });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dinonaktifkan));

            using var pembaca = database.CreateContext();
            var sesudah = await pembaca.Set<TrxPatientAssessment>().SingleAsync(x => x.Id == pengkajian.Id);

            Assert.Equal(kebijakan.Id, sesudah.PolicyId);
            Assert.Equal(pengkajian.DueAt, sesudah.DueAt);
        }

        /// <summary>
        /// Kebijakan yang sudah dipakai menilai pengkajian tidak dapat dihapus; penolakannya
        /// menyarankan menonaktifkannya.
        /// </summary>
        [Fact]
        public async Task KebijakanYangDipakai_TidakDapatDihapus()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var admin = RekamMedisTestData.BuatPengguna(context, "governance");
            var controller = BuatController(context, admin.Id);

            await controller.Create(Permintaan());

            var kebijakan = await context.Set<MstClinicalAssessmentPolicy>().SingleAsync();
            var k = RawatInapTestData.SiapkanPerawatan(context);

            context.Set<TrxPatientAssessment>().Add(new TrxPatientAssessment
            {
                AssessmentNumber = $"ASM-{Guid.NewGuid():N}"[..20],
                EncounterId = k.EncounterId,
                PatientId = k.PatientId,
                ServiceUnitId = k.ServiceUnitId,
                InpEpisodeId = k.EpisodeId,
                PolicyId = kebijakan.Id
            });
            await context.SaveChangesAsync();

            var hasil = await controller.Delete(kebijakan.Id);

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("Nonaktifkan", ControllerTestHarness.Pesan(hasil)!);

            using var pembaca = database.CreateContext();
            Assert.False((await pembaca.Set<MstClinicalAssessmentPolicy>()
                .SingleAsync(x => x.Id == kebijakan.Id)).IsDelete);
        }

        /// <summary>Kebijakan yang belum dipakai dihapus secara soft delete, bukan hard delete.</summary>
        [Fact]
        public async Task KebijakanYangBelumDipakai_DihapusSecaraSoftDelete()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var admin = RekamMedisTestData.BuatPengguna(context, "governance");
            var controller = BuatController(context, admin.Id);

            await controller.Create(Permintaan());
            var kebijakan = await context.Set<MstClinicalAssessmentPolicy>().SingleAsync();

            var hasil = await controller.Delete(kebijakan.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();
            var sesudah = await pembaca.Set<MstClinicalAssessmentPolicy>()
                .SingleAsync(x => x.Id == kebijakan.Id);

            Assert.True(sesudah.IsDelete);
            Assert.False(sesudah.IsActive);
            Assert.NotNull(sesudah.DeleteDateTime);
        }

        // =====================================================================
        // Permukaan endpoint master data
        // =====================================================================

        /// <summary>
        /// Sembilan endpoint baseline master data terpasang beserta verb dan path-nya —
        /// <c>rules/backend/master-data-endpoint-standard.md</c> bagian 1.
        /// </summary>
        [Fact]
        public void SembilanEndpointBaseline_Terpasang()
        {
            var diharapkan = new (string Verb, string? Template)[]
            {
                ("GET", "filters/metadata"),
                ("GET", "summary"),
                ("GET", null),
                ("GET", "options"),
                ("GET", "{id:guid}"),
                ("POST", null),
                ("PUT", "{id:guid}"),
                ("PATCH", "{id:guid}/status"),
                ("DELETE", "{id:guid}")
            };

            var terpasang = typeof(ClinicalAssessmentPolicyController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .SelectMany(m => m.GetCustomAttributes<HttpMethodAttribute>()
                    .Select(a => (Verb: a.HttpMethods.First(), a.Template)))
                .ToList();

            foreach (var (verb, template) in diharapkan)
            {
                Assert.True(
                    terpasang.Any(x => x.Verb == verb && x.Template == template),
                    $"Endpoint {verb} /{template} belum ada.");
            }

            Assert.Equal(diharapkan.Length, terpasang.Count);
        }

        /// <summary>
        /// Metadata penyaring menjanjikan pengurutan dan ukuran halaman yang benar-benar
        /// didukung daftar utamanya.
        /// </summary>
        [Fact]
        public void MetadataPenyaring_MenjanjikanYangBenarBenarDidukung()
        {
            var metadata = ClinicalAssessmentPolicyService.BuildFilterMetadata();

            Assert.NotEmpty(metadata.SortOptions);
            Assert.Equal(["asc", "desc"], metadata.SortDirections);
            Assert.Equal([10, 25, 50, 100], metadata.PageSizeOptions);
            Assert.Equal(6, metadata.AssessmentTypeOptions.Count);
            Assert.NotEmpty(metadata.ServiceUnitTypeOptions);
            Assert.NotEmpty(metadata.CreateFields);
            Assert.NotEmpty(metadata.UpdateFields);
            Assert.Equal("effectiveFrom", metadata.DefaultFilter.SortBy);
            Assert.Equal(25, metadata.DefaultFilter.PageSize);
        }

        /// <summary>Daftar utama menyaring, mengurutkan, dan berhalaman sebagaimana dijanjikan.</summary>
        [Fact]
        public async Task DaftarUtama_MenyaringDanBerhalaman()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var admin = RekamMedisTestData.BuatPengguna(context, "governance");
            var controller = BuatController(context, admin.Id);

            await controller.Create(Permintaan(kode: "KEP-A", menit: 1440));
            await controller.Create(Permintaan(kode: "KEP-B", menit: 480,
                jenisPelayanan: ServiceUnitType.Inpatient));

            var hasil = await controller.GetAll(
                search: null, isActive: true, assessmentType: PatientAssessmentType.Initial,
                serviceUnitType: ServiceUnitType.Inpatient, onlyCurrentlyEffective: true,
                startDate: null, endDate: null, sortBy: "policyCode", sortDirection: "asc",
                pageNumber: 1, pageSize: 25);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var isi = (ObjectResult)hasil;
            var data = isi.Value!.GetType().GetProperty("Data")!.GetValue(isi.Value)!;
            var totalData = (int)data.GetType().GetProperty("TotalData")!.GetValue(data)!;

            Assert.Equal(1, totalData);
        }

        /// <summary>Rentang tanggal terbalik ditolak <c>400</c>, bukan mengembalikan daftar kosong.</summary>
        [Fact]
        public async Task RentangTanggalTerbalik_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var admin = RekamMedisTestData.BuatPengguna(context, "governance");

            var hasil = await BuatController(context, admin.Id).GetAll(
                search: null, isActive: null, assessmentType: null, serviceUnitType: null,
                onlyCurrentlyEffective: null,
                startDate: new DateTime(2026, 9, 20, 0, 0, 0, DateTimeKind.Utc),
                endDate: new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                sortBy: null, sortDirection: null, pageNumber: 1, pageSize: 25);

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
        }

        /// <summary>Bentuk tabel master sesuai kamus data: kode unik dan index pemilihan.</summary>
        [Fact]
        public void BentukTabelMaster_SesuaiKamusData()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var entity = context.Model.FindEntityType(typeof(MstClinicalAssessmentPolicy))!;

            Assert.Equal("MstClinicalAssessmentPolicy", entity.GetTableName());
            Assert.Equal("public", entity.GetSchema());

            var indexKode = entity.GetIndexes()
                .Single(x => x.Properties.Count == 1 && x.Properties[0].Name == "PolicyCode");

            Assert.True(indexKode.IsUnique);

            var adaIndexPemilihan = entity.GetIndexes().Any(x =>
                x.Properties.Select(p => p.Name)
                    .SequenceEqual(["AssessmentType", "ServiceUnitType", "EffectiveFrom"]));

            Assert.True(adaIndexPemilihan,
                "Index pemilihan kebijakan (AssessmentType, ServiceUnitType, EffectiveFrom) tidak ada.");

            Assert.True(entity.FindProperty("ServiceUnitType")!.IsNullable);
            Assert.True(entity.FindProperty("EffectiveTo")!.IsNullable);
            Assert.False(entity.FindProperty("DueWithinMinutes")!.IsNullable);
        }
    }
}
