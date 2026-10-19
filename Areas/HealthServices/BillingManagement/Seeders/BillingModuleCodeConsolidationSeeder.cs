using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Seeders
{
    /// <summary>
    /// Memindahkan registry izin Billing dari tujuh kode modul lama ke kode kanonik
    /// <c>HEALTH_SERVICE_BILLING_MANAGEMENT</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Pola dan alasannya identik dengan
    /// <see cref="PharmacyManagement.Seeders.PharmacyModuleCodeConsolidationSeeder"/> — baca
    /// dokumentasi di sana untuk penjelasan lengkap kenapa ini harus berjalan sebelum
    /// <see cref="Seeders.AccessMenuSeeder"/> dan kenapa sekadar mengganti literal
    /// <c>moduleCode</c> pada atribut <c>[AccessController]</c> lalu redeploy itu berbahaya:
    /// <c>AccessMenuSeeder</c> mengenali satu baris <c>SysControllerAccess</c> dari pasangan
    /// <c>(ModuleId, ControllerName)</c>, sehingga perubahan <c>ModuleCode</c> tanpa migrasi ini
    /// akan membuat baris baru dan menutup baris lama — setiap <c>SysAccessPolicy</c> yang
    /// sudah diberikan admin ke controller Billing yang terdampak akan kehilangan acuan tanpa
    /// satu pun galat.
    /// </para>
    /// <para>
    /// <b>Beda dari kasus Farmasi:</b> Farmasi hanya punya satu kode lama. Billing punya delapan
    /// controller yang terpecah ke tujuh kode modul berbeda (<see cref="KodeLegacyList"/>) dengan
    /// nama tampilan yang sama persis "Health Service Billing Management" — ISS-BKC-004-01. Yang
    /// dilakukan di sini hanyalah memindahkan relasi modulnya secara berurutan untuk setiap kode
    /// lama ke satu modul kanonik yang sama; tidak ada baris yang dihapus, dan tidak ada
    /// <c>SysAccessPolicy</c> yang dibuang.
    /// </para>
    /// <para>
    /// Sengaja TIDAK mencakup <c>HEALTH_SERVICE_BILLING_MANAGEMENT_MASTER_DATA</c> maupun
    /// <c>HEALTH_SERVICE_BILLING_MANAGEMENT_PETTY_CASH</c> — keduanya dikonfirmasi tetap terpisah
    /// sebagai kartu tersendiri (keputusan K-01, <c>issue-004-kartu-billing-ganda-manajemen-role.md</c>).
    /// </para>
    /// </remarks>
    public static class BillingModuleCodeConsolidationSeeder
    {
        public const string KodeKanonik = "HEALTH_SERVICE_BILLING_MANAGEMENT";

        public static readonly IReadOnlyList<string> KodeLegacyList =
        [
            "HEALTH_SERVICE_BILLING_MANAGEMENT_CONSUMER_HANDOFF",
            "HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING",
            "HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_FINALIZATION",
            "HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_FINANCIAL_EXCEPTION",
            "HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_SETTLEMENT",
            "HEALTH_SERVICE_BILLING_MANAGEMENT_INPATIENT",
            "HEALTH_SERVICE_BILLING_MANAGEMENT_CASHIER_SHIFT"
        ];

        /// <param name="KodeLegacy">Kode modul lama yang dikonsolidasikan pada baris ini.</param>
        /// <param name="ControllerDipindahkan">
        /// Baris yang berpindah modul dengan <c>Id</c> dipertahankan — jalur normal.
        /// </param>
        /// <param name="ControllerDigabungkan">
        /// Baris lama yang namanya sudah ada di modul kanonik, sehingga tidak dapat dipindahkan
        /// dan harus digabungkan. Normalnya nol.
        /// </param>
        /// <param name="PolicyDialihkan">
        /// Jumlah <c>SysAccessPolicy</c> yang acuannya dialihkan ke baris kanonik akibat
        /// penggabungan. Normalnya nol.
        /// </param>
        /// <param name="PolicyTerdampak">
        /// Jumlah <c>SysAccessPolicy</c> yang menunjuk controller Billing lama pada kode ini.
        /// Dicatat untuk laporan: pada jalur normal angkanya bisa besar dan tetap <b>tidak</b>
        /// disentuh.
        /// </param>
        /// <param name="ModulLegacyDinonaktifkan">
        /// Benar hanya bila modul lama ini sudah tidak memiliki controller aktif sama sekali.
        /// </param>
        public sealed record HasilPerKodeLegacy(
            string KodeLegacy,
            int ControllerDipindahkan,
            int ControllerDigabungkan,
            int PolicyDialihkan,
            int PolicyTerdampak,
            bool ModulLegacyDinonaktifkan);

        public sealed record HasilKonsolidasi(IReadOnlyList<HasilPerKodeLegacy> PerKode)
        {
            public int TotalControllerDipindahkan => PerKode.Sum(x => x.ControllerDipindahkan);
            public int TotalControllerDigabungkan => PerKode.Sum(x => x.ControllerDigabungkan);
            public int TotalPolicyDialihkan => PerKode.Sum(x => x.PolicyDialihkan);
        }

        public static async Task<HasilKonsolidasi> SeedAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await ConsolidateAsync(dbContext, cancellationToken);
        }

        /// <summary>
        /// Dipisahkan dari <see cref="SeedAsync"/> supaya dapat diuji tanpa host penuh, mengikuti
        /// pola <c>PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync</c>.
        /// </summary>
        public static async Task<HasilKonsolidasi> ConsolidateAsync(
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var modulKanonik = await dbContext.SysApplicationModules
                .SingleOrDefaultAsync(x => x.ModuleCode == KodeKanonik, cancellationToken);

            // Modul kanonik normalnya sudah ada karena empat controller memakainya sejak awal
            // (BillingChargeReconciliation, EncounterBillingSummary, BillingFolio,
            // PatientBillingSummary). Bila sebuah lingkungan belum mengenalnya sama sekali,
            // dibuat di sini supaya perpindahan tetap dapat berjalan tanpa menunggu
            // AccessMenuSeeder - yang justru berjalan sesudah ini.
            if (modulKanonik == null)
            {
                modulKanonik = new SysApplicationModule
                {
                    Id = Guid.NewGuid(),
                    ModuleCode = KodeKanonik,
                    ModuleName = "Health Service Billing Management",
                    AreaName = "HealthServices",
                    IsActive = true,
                    IsDelete = false,
                    IsCancel = false,
                    CreateDateTime = now
                };

                dbContext.SysApplicationModules.Add(modulKanonik);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var hasilPerKode = new List<HasilPerKodeLegacy>();

            foreach (var kodeLegacy in KodeLegacyList)
            {
                hasilPerKode.Add(await KonsolidasikanSatuKodeAsync(
                    dbContext, kodeLegacy, modulKanonik, now, cancellationToken));
            }

            return new HasilKonsolidasi(hasilPerKode);
        }

        private static async Task<HasilPerKodeLegacy> KonsolidasikanSatuKodeAsync(
            ApplicationDbContext dbContext,
            string kodeLegacy,
            SysApplicationModule modulKanonik,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var modulLegacy = await dbContext.SysApplicationModules
                .SingleOrDefaultAsync(x => x.ModuleCode == kodeLegacy, cancellationToken);

            // Lingkungan baru tidak pernah memiliki modul lama ini. Tidak ada yang perlu
            // dipindahkan.
            if (modulLegacy == null)
            {
                return new HasilPerKodeLegacy(kodeLegacy, 0, 0, 0, 0, false);
            }

            var controllerLegacy = await dbContext.SysControllerAccesses
                .Where(x => x.ModuleId == modulLegacy.Id)
                .ToListAsync(cancellationToken);

            if (controllerLegacy.Count == 0)
            {
                // Sudah pernah dijalankan. Sisa pekerjaannya hanya menutup modulnya.
                var ditutup = await NonaktifkanModulLegacyAsync(
                    dbContext, modulLegacy, now, cancellationToken);

                return new HasilPerKodeLegacy(kodeLegacy, 0, 0, 0, 0, ditutup);
            }

            var idLegacy = controllerLegacy.Select(x => x.Id).ToList();

            var aksiLegacy = await dbContext.SysActionAccesses
                .Where(x => idLegacy.Contains(x.ControllerAccessId))
                .ToListAsync(cancellationToken);

            var idAksiLegacy = aksiLegacy.Select(x => x.Id).ToList();

            var policyTerdampak = await dbContext.SysAccessPolicies
                .Where(x =>
                    idLegacy.Contains(x.ControllerAccessId) ||
                    idAksiLegacy.Contains(x.ActionAccessId))
                .ToListAsync(cancellationToken);

            // Nama controller yang SUDAH ada di modul kanonik. Memindahkan baris lama dengan
            // nama yang sama akan melanggar indeks unik (ModuleId, ControllerName), jadi
            // keduanya harus digabungkan alih-alih dipindahkan.
            var kanonikByNama = await dbContext.SysControllerAccesses
                .Where(x => x.ModuleId == modulKanonik.Id)
                .ToDictionaryAsync(x => x.ControllerName, x => x, StringComparer.Ordinal,
                    cancellationToken);

            var dipindahkan = 0;
            var digabungkan = 0;
            var policyDialihkan = 0;

            foreach (var controller in controllerLegacy)
            {
                if (!kanonikByNama.TryGetValue(controller.ControllerName, out var kembar))
                {
                    // Jalur normal: cukup pindahkan relasi modulnya. Id tidak berubah, sehingga
                    // seluruh aksi dan policy yang menunjuknya tetap sah tanpa disentuh.
                    controller.ModuleId = modulKanonik.Id;
                    controller.UpdateDateTime = now;

                    kanonikByNama.Add(controller.ControllerName, controller);
                    dipindahkan++;
                    continue;
                }

                // Jalur penggabungan. Hanya terjadi bila sebuah lingkungan pernah mendaftarkan
                // controller yang sama di kedua modul. Source tidak dapat menghasilkan keadaan
                // ini, tetapi data lama bisa.
                policyDialihkan += await GabungkanAsync(
                    dbContext, controller, kembar, aksiLegacy, now, cancellationToken);

                controller.IsActive = false;
                controller.IsDelete = true;
                controller.DeleteDateTime = now;
                controller.UpdateDateTime = now;
                digabungkan++;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            var modulDinonaktifkan = await NonaktifkanModulLegacyAsync(
                dbContext, modulLegacy, now, cancellationToken);

            return new HasilPerKodeLegacy(
                kodeLegacy,
                dipindahkan,
                digabungkan,
                policyDialihkan,
                policyTerdampak.Count,
                modulDinonaktifkan);
        }

        /// <summary>
        /// Mengalihkan aksi dan policy dari satu controller lama ke kembarannya di modul
        /// kanonik, tanpa membuang satu pun policy.
        /// </summary>
        /// <returns>Jumlah policy yang acuannya dialihkan.</returns>
        private static async Task<int> GabungkanAsync(
            ApplicationDbContext dbContext,
            SysControllerAccess asal,
            SysControllerAccess tujuan,
            List<SysActionAccess> aksiLegacy,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var aksiTujuan = await dbContext.SysActionAccesses
                .Where(x => x.ControllerAccessId == tujuan.Id)
                .ToDictionaryAsync(x => x.ActionName, x => x, StringComparer.Ordinal,
                    cancellationToken);

            var dialihkan = 0;

            foreach (var aksi in aksiLegacy.Where(x => x.ControllerAccessId == asal.Id))
            {
                if (!aksiTujuan.TryGetValue(aksi.ActionName, out var aksiKembar))
                {
                    // Tujuan belum punya aksi bernama itu: pindahkan aksinya apa adanya.
                    // Id aksi tidak berubah, jadi policy yang menunjuknya tetap sah.
                    aksi.ControllerAccessId = tujuan.Id;
                    aksi.UpdateDateTime = now;
                    aksiTujuan.Add(aksi.ActionName, aksi);
                    continue;
                }

                // Keduanya ada. Policy yang menunjuk aksi lama dialihkan ke aksi kembar,
                // kecuali bila peran yang sama sudah memilikinya - mengalihkannya akan
                // melanggar indeks unik (DepartmentId, PositionId, ControllerAccessId,
                // ActionAccessId). Yang sudah ada dibiarkan, bukan ditimpa, karena keduanya
                // menyatakan izin yang sama.
                var policyLama = await dbContext.SysAccessPolicies
                    .Where(x => x.ActionAccessId == aksi.Id)
                    .ToListAsync(cancellationToken);

                foreach (var policy in policyLama)
                {
                    var sudahAda = await dbContext.SysAccessPolicies.AnyAsync(x =>
                        x.DepartmentId == policy.DepartmentId &&
                        x.PositionId == policy.PositionId &&
                        x.ControllerAccessId == tujuan.Id &&
                        x.ActionAccessId == aksiKembar.Id, cancellationToken);

                    if (sudahAda) continue;

                    policy.ControllerAccessId = tujuan.Id;
                    policy.ActionAccessId = aksiKembar.Id;
                    policy.UpdateDateTime = now;
                    dialihkan++;
                }

                aksi.IsActive = false;
                aksi.IsDelete = true;
                aksi.DeleteDateTime = now;
                aksi.UpdateDateTime = now;
            }

            // Policy yang menunjuk controller lama tetapi aksinya sudah berpindah perlu ikut
            // menunjuk controller tujuan, supaya kedua kolomnya konsisten.
            var policyControllerLama = await dbContext.SysAccessPolicies
                .Where(x => x.ControllerAccessId == asal.Id)
                .ToListAsync(cancellationToken);

            foreach (var policy in policyControllerLama)
            {
                policy.ControllerAccessId = tujuan.Id;
                policy.UpdateDateTime = now;
            }

            return dialihkan;
        }

        /// <summary>
        /// Menonaktifkan modul lama, dan <b>hanya</b> bila ia sudah tidak memiliki controller
        /// aktif serta tidak ada policy yang masih menunjuk isinya.
        /// </summary>
        private static async Task<bool> NonaktifkanModulLegacyAsync(
            ApplicationDbContext dbContext,
            SysApplicationModule modulLegacy,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var masihTerpakai = await dbContext.SysControllerAccesses
                .AnyAsync(x => x.ModuleId == modulLegacy.Id && x.IsActive && !x.IsDelete,
                    cancellationToken);

            if (masihTerpakai) return false;

            var idTersisa = await dbContext.SysControllerAccesses
                .Where(x => x.ModuleId == modulLegacy.Id)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            // Walaupun controllernya sudah ditutup, policy yang masih menunjuknya berarti ada
            // izin yang akan kehilangan acuan bila modulnya ikut ditutup. Dalam keadaan itu
            // modulnya dibiarkan apa adanya dan dilaporkan sebagai belum aman.
            if (idTersisa.Count > 0)
            {
                var adaPolicy = await dbContext.SysAccessPolicies
                    .AnyAsync(x => idTersisa.Contains(x.ControllerAccessId), cancellationToken);

                if (adaPolicy) return false;
            }

            if (!modulLegacy.IsActive) return false;

            modulLegacy.IsActive = false;
            modulLegacy.UpdateDateTime = now;

            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
