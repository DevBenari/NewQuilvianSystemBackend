using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Seeders
{
    /// <summary>
    /// Memindahkan registry izin Farmasi dari kode modul lama
    /// <c>HEALTH_SERVICE_PHARMACY</c> ke kode kanonik
    /// <c>HEALTH_SERVICE_PHARMACY_MANAGEMENT</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Mengapa ini harus berjalan sebelum <see cref="Seeders.AccessMenuSeeder"/>, bukan
    /// dibiarkan diurus olehnya.</b>
    /// </para>
    /// <para>
    /// <c>AccessMenuSeeder</c> mengenali satu baris <c>SysControllerAccess</c> dari pasangan
    /// <c>(ModuleId, ControllerName)</c>. Begitu 12 controller Farmasi berganti kode modul,
    /// pasangan itu berubah, sehingga seeder tidak lagi menemukan baris yang lama: ia
    /// <b>membuat baris baru dengan <c>Id</c> baru</b>, lalu menutup baris lama
    /// (<c>IsActive = false</c>, <c>IsDelete = true</c>).
    /// </para>
    /// <para>
    /// Akibatnya fatal dan senyap. <c>SysAccessPolicy</c> menunjuk
    /// <c>ControllerAccessId</c> dan <c>ActionAccessId</c>; keduanya akan tetap menunjuk baris
    /// yang sudah ditutup, dan <c>AccessPermissionService</c> menyaring
    /// <c>IsActive &amp;&amp; !IsDelete</c>. Jadi setiap izin Farmasi yang pernah diberikan
    /// administrator akan berhenti berlaku tanpa satu pun galat — peran yang tadinya boleh
    /// menyerahkan obat mendadak menerima <c>403</c>, dan penyebabnya tidak terlihat di mana
    /// pun.
    /// </para>
    /// <para>
    /// Karena itu yang dilakukan di sini <b>hanya memindahkan relasi modulnya</b>:
    /// <c>SysControllerAccess.ModuleId</c> ditulis ulang ke modul kanonik, sementara
    /// <c>Id</c>-nya dipertahankan. Dengan begitu <c>SysActionAccess.ControllerAccessId</c> dan
    /// kedua kolom pada <c>SysAccessPolicy</c> tetap sah tanpa disentuh sama sekali, dan
    /// <c>AccessMenuSeeder</c> sesudahnya menemukan barisnya lewat kunci baru lalu memperbaruinya
    /// di tempat.
    /// </para>
    /// <para>
    /// <b>Tidak ada baris yang dihapus.</b> Tidak ada <c>SysAccessPolicy</c> yang disentuh,
    /// dibuat, atau dibuang. Seeder ini aman dijalankan berulang kali: setelah perpindahan
    /// pertama, modul lama tidak lagi memiliki controller dan seluruh langkah menjadi nol
    /// pekerjaan.
    /// </para>
    /// </remarks>
    public static class PharmacyModuleCodeConsolidationSeeder
    {
        public const string KodeLegacy = "HEALTH_SERVICE_PHARMACY";
        public const string KodeKanonik = "HEALTH_SERVICE_PHARMACY_MANAGEMENT";

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
        /// Jumlah <c>SysAccessPolicy</c> yang menunjuk controller Farmasi lama. Dicatat untuk
        /// laporan: pada jalur normal angkanya bisa besar dan tetap <b>tidak</b> disentuh.
        /// </param>
        /// <param name="ModulLegacyDinonaktifkan">
        /// Benar hanya bila modul lama sudah tidak memiliki controller aktif sama sekali.
        /// </param>
        public sealed record HasilKonsolidasi(
            int ControllerDipindahkan,
            int ControllerDigabungkan,
            int PolicyDialihkan,
            int PolicyTerdampak,
            bool ModulLegacyDinonaktifkan);

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
        /// pola <c>AccessMenuSeeder.ReconcileAsync</c>.
        /// </summary>
        public static async Task<HasilKonsolidasi> ConsolidateAsync(
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var modulLegacy = await dbContext.SysApplicationModules
                .SingleOrDefaultAsync(x => x.ModuleCode == KodeLegacy, cancellationToken);

            // Lingkungan baru tidak pernah memiliki modul lama. Tidak ada yang perlu dipindahkan.
            if (modulLegacy == null)
            {
                return new HasilKonsolidasi(0, 0, 0, 0, false);
            }

            var modulKanonik = await dbContext.SysApplicationModules
                .SingleOrDefaultAsync(x => x.ModuleCode == KodeKanonik, cancellationToken);

            // Modul kanonik normalnya sudah ada karena 11 controller memakainya sejak awal.
            // Bila sebuah lingkungan hanya mengenal kode lama, modulnya dibuat di sini supaya
            // perpindahan tetap dapat berjalan tanpa menunggu AccessMenuSeeder — yang justru
            // berjalan sesudah ini.
            if (modulKanonik == null)
            {
                modulKanonik = new SysApplicationModule
                {
                    Id = Guid.NewGuid(),
                    ModuleCode = KodeKanonik,
                    ModuleName = "Health Service Pharmacy Management",
                    AreaName = modulLegacy.AreaName,
                    SortOrder = modulLegacy.SortOrder,
                    IsActive = true,
                    IsDelete = false,
                    IsCancel = false,
                    CreateDateTime = now
                };

                dbContext.SysApplicationModules.Add(modulKanonik);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var controllerLegacy = await dbContext.SysControllerAccesses
                .Where(x => x.ModuleId == modulLegacy.Id)
                .ToListAsync(cancellationToken);

            if (controllerLegacy.Count == 0)
            {
                // Sudah pernah dijalankan. Sisa pekerjaannya hanya menutup modulnya.
                var ditutup = await NonaktifkanModulLegacyAsync(
                    dbContext, modulLegacy, now, cancellationToken);

                return new HasilKonsolidasi(0, 0, 0, 0, ditutup);
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

            return new HasilKonsolidasi(
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
                // kecuali bila peran yang sama sudah memilikinya — mengalihkannya akan
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
