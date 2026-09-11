using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Seeders
{
    /// <summary>
    /// Mengisi data master awal Radiologi — <c>BE-RAD-15</c>.
    ///
    /// <b>Tiga tabel, dan hanya dua di antaranya benar-benar terisi.</b>
    ///
    /// <list type="number">
    /// <item><b>Alat pencitraan.</b> Enam alat sesuai <c>RAD-DEC-002</c> yang sudah disetujui,
    /// dengan kode mengikuti kode modalitas DICOM yang berlaku umum. Itu kosakata teknis
    /// internasional, bukan kebijakan rumah sakit.</item>
    /// <item><b>Butir keselamatan.</b> Empat butir sesuai <c>RAD-ARCH-BE-001</c> bagian 9. Ini
    /// kosakata juga: sebuah butir yang terdaftar belum menanyakan apa pun kepada pasien.
    /// Setiap barisnya membawa <c>SourceNote</c> yang menyatakan asal-usulnya apa adanya.</item>
    /// <item><b>Aturan keselamatan — disusun sebagai <c>Draft</c>, tidak pernah
    /// <c>Active</c>.</b></item>
    /// </list>
    ///
    /// <b>Mengapa aturannya berhenti di <c>Draft</c>.</b> Aturan keselamatan menentukan
    /// pertanyaan apa yang wajib dijawab sebelum seorang pasien disinari. <c>RJ-BIL-DEC-014</c>
    /// menyatakan daftar akhirnya mengikuti SOP dan otoritas klinis, dan <c>DEC-RAD-005</c>
    /// masih <c>OPEN</c> dengan pemilik tata kelola klinis. Seeder yang menerbitkan aturan
    /// berstatus <c>Active</c> berarti sebuah program menetapkan kapan pasien boleh disinari —
    /// dan itu bukan wewenang program.
    ///
    /// Usulan baseline pada <c>DEC-RAD-005</c> sendiri berbunyi: sediakan data bawaan sebagai
    /// usulan berstatus draf, sehingga tetap harus disahkan penanggung jawab klinis sebelum
    /// berlaku. Itulah yang dikerjakan di sini.
    ///
    /// <b>Akibat yang harus diketahui.</b> Sesudah seeder ini berjalan,
    /// <c>GET /rad-safety-rules/coverage</c> <b>tetap</b> mengembalikan keenam alat, dan
    /// seluruh pemeriksaan tetap ditolak. Itu bukan kegagalan seeder; itu sifat fail-closed
    /// yang bekerja sebagaimana mestinya. Modul menjadi dapat dipakai ketika penanggung jawab
    /// klinis memeriksa draf-draf ini lalu mengesahkannya lewat
    /// <c>POST /rad-safety-rules/{id}/approve</c>.
    ///
    /// <b>Yang sengaja tidak dikerjakan.</b> Seeder hanya menambah baris yang belum ada, dan
    /// tidak pernah menimpa yang sudah tersimpan — termasuk yang sudah ditandai terhapus.
    /// Nama, penanda, urutan tampil, dan keputusan pengesahan adalah milik penggunanya;
    /// menimpanya setiap kali server menyala berarti membatalkan keputusan mereka diam-diam.
    /// </summary>
    public static class RadiologyMasterDataSeeder
    {
        private const string SumberButir =
            "Baseline implementasi RAD-ARCH-BE-001 bagian 9. Bukan SOP rumah sakit yang sudah " +
            "disahkan; wajib diverifikasi terhadap SOP yang berlaku.";

        private const string CatatanBerkontras =
            "Usulan: wajib hanya bila pemeriksaannya memakai media kontras. Syarat itu belum " +
            "dapat dinyatakan model aturan saat ini, sehingga baris ini disusun tidak wajib. " +
            "Penanggung jawab klinis dapat mengubahnya menjadi wajib, atau memecahnya menjadi " +
            "aturan per pemeriksaan berkontras, sebelum mengesahkan.";

        private const string CatatanUsg =
            "Usulan: USG tidak memakai radiasi pengion dan tidak punya butir wajib pada " +
            "RAD-ARCH-BE-001 bagian 9. Baris ini tetap disediakan karena gerbang bersifat " +
            "fail-closed — tanpa satu pun aturan berlaku, seluruh pemeriksaan USG akan ditolak. " +
            "Ganti butirnya bila SOP rumah sakit menetapkan yang lain.";

        /// <summary>
        /// Enam alat sesuai <c>RAD-DEC-002</c>. <c>Id</c> ditetapkan tetap supaya sebuah alat
        /// punya identitas yang sama di setiap lingkungan dan dapat dirujuk dengan pasti.
        /// </summary>
        private static readonly BaselineAlat[] BaselineAlatList =
        {
            new("a1d7e100-0001-4c10-9a01-7c2d0b6e8d01", "CR", "X-Ray", true, false, 1,
                "Radiografi umum"),
            new("a1d7e100-0002-4c10-9a01-7c2d0b6e8d02", "CT", "CT-Scan", true, true, 2,
                "Computed Tomography"),
            new("a1d7e100-0003-4c10-9a01-7c2d0b6e8d03", "MR", "MRI", false, true, 3,
                "Magnetic Resonance Imaging"),
            new("a1d7e100-0004-4c10-9a01-7c2d0b6e8d04", "US", "USG", false, false, 4,
                "Ultrasonografi"),
            new("a1d7e100-0005-4c10-9a01-7c2d0b6e8d05", "MG", "Mamografi", true, false, 5,
                "Mammography"),
            new("a1d7e100-0006-4c10-9a01-7c2d0b6e8d06", "RF", "Fluoroskopi", true, true, 6,
                "Radiofluoroscopy"),
        };

        /// <summary>Empat butir sesuai <c>RAD-ARCH-BE-001</c> bagian 9.</summary>
        private static readonly BaselineButir[] BaselineButirList =
        {
            new("b2e8f200-0001-4d20-8b02-8d3e1c7f9e01", "PREGNANCY_SCREENING",
                "Skrining kehamilan", "Radiation", false,
                "Menanyakan kemungkinan kehamilan sebelum pemeriksaan dengan radiasi pengion.",
                1),
            new("b2e8f200-0002-4d20-8b02-8d3e1c7f9e02", "METAL_IMPLANT_SCREENING",
                "Skrining implan logam atau alat pacu jantung", "Implant", true,
                "Menanyakan keberadaan implan logam, alat pacu jantung, atau benda logam lain " +
                "di dalam tubuh.",
                2),
            new("b2e8f200-0003-4d20-8b02-8d3e1c7f9e03", "CONTRAST_ALLERGY",
                "Riwayat alergi media kontras", "Contrast", true,
                "Menanyakan riwayat reaksi alergi terhadap media kontras.",
                3),
            new("b2e8f200-0004-4d20-8b02-8d3e1c7f9e04", "RENAL_FUNCTION",
                "Pemeriksaan fungsi ginjal sebelum kontras", "Contrast", true,
                "Memastikan fungsi ginjal sudah dinilai sebelum media kontras diberikan.",
                4),
        };

        /// <summary>
        /// Usulan pemetaan butir per alat, <c>RAD-ARCH-BE-001</c> bagian 9.
        ///
        /// Seluruhnya disusun berstatus <c>Draft</c>. Tidak satu pun berlaku sebelum disahkan.
        /// </summary>
        private static readonly BaselineAturan[] BaselineAturanList =
        {
            new("c3f9a300-0001-4e30-9c03-9e4f2d8a0f01", "CR", "PREGNANCY_SCREENING", true, null),

            new("c3f9a300-0002-4e30-9c03-9e4f2d8a0f02", "CT", "PREGNANCY_SCREENING", true, null),
            new("c3f9a300-0003-4e30-9c03-9e4f2d8a0f03", "CT", "CONTRAST_ALLERGY", false,
                CatatanBerkontras),
            new("c3f9a300-0004-4e30-9c03-9e4f2d8a0f04", "CT", "RENAL_FUNCTION", false,
                CatatanBerkontras),

            new("c3f9a300-0005-4e30-9c03-9e4f2d8a0f05", "MR", "METAL_IMPLANT_SCREENING", true, null),
            new("c3f9a300-0006-4e30-9c03-9e4f2d8a0f06", "MR", "CONTRAST_ALLERGY", false,
                CatatanBerkontras),
            new("c3f9a300-0007-4e30-9c03-9e4f2d8a0f07", "MR", "RENAL_FUNCTION", false,
                CatatanBerkontras),

            new("c3f9a300-0008-4e30-9c03-9e4f2d8a0f08", "US", "PREGNANCY_SCREENING", false,
                CatatanUsg),

            new("c3f9a300-0009-4e30-9c03-9e4f2d8a0f09", "MG", "PREGNANCY_SCREENING", true, null),

            new("c3f9a300-0010-4e30-9c03-9e4f2d8a0f10", "RF", "PREGNANCY_SCREENING", true, null),
            new("c3f9a300-0011-4e30-9c03-9e4f2d8a0f11", "RF", "CONTRAST_ALLERGY", false,
                CatatanBerkontras),
            new("c3f9a300-0012-4e30-9c03-9e4f2d8a0f12", "RF", "RENAL_FUNCTION", false,
                CatatanBerkontras),
        };

        public static async Task SeedAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(RadiologyMasterDataSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;

            if (!seedEnabled)
            {
                logger.LogInformation(
                    "Seeder data master radiologi dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            await SeedAsync(dbContext, logger, cancellationToken);
        }

        /// <summary>
        /// Menjalankan pengisian terhadap satu <see cref="ApplicationDbContext"/> yang sudah
        /// ada. Dipisahkan agar perilakunya dapat diuji tanpa membangun service provider.
        /// </summary>
        public static async Task<SeedResult> SeedAsync(
            ApplicationDbContext dbContext,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var alatBaru = await IsiAlatAsync(dbContext, now, cancellationToken);
            var butirBaru = await IsiButirAsync(dbContext, now, cancellationToken);

            if (alatBaru > 0 || butirBaru > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var aturanBaru = await IsiAturanDrafAsync(dbContext, now, cancellationToken);

            if (aturanBaru > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var alatBelumTercakup = await AlatBelumTercakupAsync(dbContext, now, cancellationToken);

            logger.LogInformation(
                "Seeder data master radiologi menambahkan {Alat} alat, {Butir} butir " +
                "keselamatan, dan {Aturan} draf aturan.",
                alatBaru, butirBaru, aturanBaru);

            if (alatBelumTercakup > 0)
            {
                // Peringatan ini sengaja ditulis setiap kali, bukan hanya saat seeder mengisi
                // sesuatu. Selama angkanya bukan nol, modul belum dapat dipakai memeriksa
                // pasien — dan itu perlu terbaca di log setiap kali server menyala.
                logger.LogWarning(
                    "{Jumlah} alat pencitraan belum punya aturan keselamatan yang BERLAKU. " +
                    "Seluruh pemeriksaan pada alat tersebut akan ditolak sampai penanggung " +
                    "jawab klinis mengesahkan aturannya. Ini perilaku fail-closed yang " +
                    "disengaja, bukan kesalahan konfigurasi.",
                    alatBelumTercakup);
            }

            return new SeedResult(alatBaru, butirBaru, aturanBaru, alatBelumTercakup);
        }

        private static async Task<int> IsiAlatAsync(
            ApplicationDbContext dbContext,
            DateTime now,
            CancellationToken cancellationToken)
        {
            // Baris yang sudah ditandai terhapus ikut dihitung, supaya kodenya tidak diisi ulang.
            var kodeAda = new HashSet<string>(
                await dbContext.MstRadModalities
                    .AsNoTracking()
                    .Select(x => x.ModalityCode)
                    .ToListAsync(cancellationToken),
                StringComparer.OrdinalIgnoreCase);

            var ditambah = 0;

            foreach (var alat in BaselineAlatList)
            {
                if (kodeAda.Contains(alat.Kode))
                {
                    continue;
                }

                dbContext.MstRadModalities.Add(new MstRadModality
                {
                    Id = Guid.Parse(alat.Id),
                    ModalityCode = alat.Kode,
                    ModalityName = alat.Nama,
                    Description = alat.Keterangan,
                    UsesIonisingRadiation = alat.Pengion,
                    SupportsContrast = alat.Kontras,
                    IsActive = true,
                    SortOrder = alat.Urutan,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty,
                });

                ditambah++;
            }

            return ditambah;
        }

        private static async Task<int> IsiButirAsync(
            ApplicationDbContext dbContext,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var kodeAda = new HashSet<string>(
                await dbContext.MstRadSafetyRequirements
                    .AsNoTracking()
                    .Select(x => x.RequirementCode)
                    .ToListAsync(cancellationToken),
                StringComparer.OrdinalIgnoreCase);

            var ditambah = 0;

            foreach (var butir in BaselineButirList)
            {
                if (kodeAda.Contains(butir.Kode))
                {
                    continue;
                }

                dbContext.MstRadSafetyRequirements.Add(new MstRadSafetyRequirement
                {
                    Id = Guid.Parse(butir.Id),
                    RequirementCode = butir.Kode,
                    RequirementName = butir.Nama,
                    Category = butir.Kelompok,
                    Description = butir.Keterangan,
                    RequiresNote = butir.WajibCatatan,
                    SourceNote = SumberButir,
                    IsActive = true,
                    SortOrder = butir.Urutan,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty,
                });

                ditambah++;
            }

            return ditambah;
        }

        /// <summary>
        /// Menyusun usulan aturan sebagai <b>draf</b>.
        ///
        /// Tidak satu baris pun berstatus <c>Active</c>. Selama belum disahkan penanggung jawab
        /// klinis, seluruhnya tidak berpengaruh pada satu pemeriksaan pun.
        /// </summary>
        private static async Task<int> IsiAturanDrafAsync(
            ApplicationDbContext dbContext,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var alatPerKode = await dbContext.MstRadModalities
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .ToDictionaryAsync(x => x.ModalityCode, x => x.Id, cancellationToken);

            var butirPerKode = await dbContext.MstRadSafetyRequirements
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .ToDictionaryAsync(x => x.RequirementCode, x => x.Id, cancellationToken);

            var aturanAda = new HashSet<Guid>(
                await dbContext.MstRadModalitySafetyRules
                    .AsNoTracking()
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken));

            // Kombinasi yang sudah punya baris apa pun tidak diusulkan ulang, walaupun barisnya
            // dibuat orang lain dengan Id yang berbeda. Mengusulkan dua kali untuk kombinasi
            // yang sama hanya menambah pekerjaan penanggung jawab klinis.
            var pasangan = await dbContext.MstRadModalitySafetyRules
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ProcedureId == null)
                .Select(x => new { x.ModalityId, x.SafetyRequirementId })
                .ToListAsync(cancellationToken);

            var kombinasiAda = new HashSet<(Guid, Guid)>(
                pasangan.Select(x => (x.ModalityId, x.SafetyRequirementId)));

            var ditambah = 0;

            foreach (var aturan in BaselineAturanList)
            {
                if (aturanAda.Contains(Guid.Parse(aturan.Id)))
                {
                    continue;
                }

                if (!alatPerKode.TryGetValue(aturan.KodeAlat, out var modalityId) ||
                    !butirPerKode.TryGetValue(aturan.KodeButir, out var requirementId))
                {
                    continue;
                }

                if (kombinasiAda.Contains((modalityId, requirementId)))
                {
                    continue;
                }

                dbContext.MstRadModalitySafetyRules.Add(new MstRadModalitySafetyRule
                {
                    Id = Guid.Parse(aturan.Id),
                    ModalityId = modalityId,
                    ProcedureId = null,
                    SafetyRequirementId = requirementId,
                    IsMandatory = aturan.Wajib,
                    EffectiveFrom = now,
                    EffectiveTo = null,
                    RuleVersion = 1,

                    // Inilah barisnya. Usulan, bukan kebijakan.
                    RuleStatus = RadSafetyRuleStatus.Draft,
                    IsActive = false,

                    Note = aturan.Catatan,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty,
                });

                kombinasiAda.Add((modalityId, requirementId));
                ditambah++;
            }

            return ditambah;
        }

        private static Task<int> AlatBelumTercakupAsync(
            ApplicationDbContext dbContext,
            DateTime now,
            CancellationToken cancellationToken) =>
            dbContext.MstRadModalities
                .AsNoTracking()
                .CountAsync(
                    x => !x.IsDelete &&
                         x.IsActive &&
                         !dbContext.MstRadModalitySafetyRules.Any(r =>
                             !r.IsDelete &&
                             r.ModalityId == x.Id &&
                             r.RuleStatus == RadSafetyRuleStatus.Active &&
                             r.EffectiveFrom <= now &&
                             (r.EffectiveTo == null || r.EffectiveTo > now)),
                    cancellationToken);

        /// <summary>Hasil satu kali pengisian, supaya dapat diperiksa uji dan dicatat log.</summary>
        public sealed record SeedResult(
            int AlatDitambahkan,
            int ButirDitambahkan,
            int DrafAturanDitambahkan,
            int AlatBelumTercakup);

        private sealed record BaselineAlat(
            string Id,
            string Kode,
            string Nama,
            bool Pengion,
            bool Kontras,
            int Urutan,
            string Keterangan);

        private sealed record BaselineButir(
            string Id,
            string Kode,
            string Nama,
            string Kelompok,
            bool WajibCatatan,
            string Keterangan,
            int Urutan);

        private sealed record BaselineAturan(
            string Id,
            string KodeAlat,
            string KodeButir,
            bool Wajib,
            string? Catatan);
    }
}
