using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders
{
    /// <summary>
    /// Mengisi data master Rawat Inap agar modul dapat dinyalakan tanpa layar pengaturan
    /// menampilkan daftar kosong. Seeder ini hanya menambah baris yang belum ada berdasarkan
    /// Code dan ItemCode, dan tidak pernah menimpa baris yang sudah tersimpan, supaya nilai
    /// yang sudah disesuaikan admin tidak hilang saat aplikasi dijalankan ulang.
    /// </summary>
    /// <remarks>
    /// Dua batas yang mengikat seeder ini, keduanya dari RWI-DEC-048:
    ///
    /// 1. Seeder MENOLAK berjalan di lingkungan produksi. Data master produksi ditetapkan
    ///    pemilik proses bisnis lewat layar admin, bukan oleh baris kode yang ikut terbawa
    ///    setiap kali aplikasi dinyalakan.
    /// 2. Seeder TIDAK PERNAH membuat baris MstRoom maupun MstBed. Susunan kamar dan tempat
    ///    tidur khas tiap rumah sakit; menebaknya menghasilkan master palsu yang terlanjur
    ///    dipakai penempatan pasien.
    ///
    /// <para>
    /// <b>Sejak <c>BE-RWI-186</c></b> seeder juga mengisi 15 butir dan 3 sub-butir Ceklist Serah
    /// Terima Pasien Baru (<c>STPB-*</c>) beserta nilai cetak V1 — kode formulir dan kota — untuk
    /// lingkungan pengembangan dan UAT. Nilai V1 itu hanya boleh ada di seeder ini
    /// (<c>RWI-AC-368</c>); service, controller, DTO, dan komponen cetak Workspace PPRI tidak
    /// memuatnya. Di produksi butir serah terima dan kode formulir diisi admin lewat layar.
    /// </para>
    /// </remarks>
    public static class InpatientMasterDataSeeder
    {
        /// <summary>Kode baris pengaturan tunggal yang dipakai seluruh modul Rawat Inap.</summary>
        public const string DefaultSettingCode = "DEFAULT";

        /// <summary>Nama lingkungan yang membuat seeder berhenti tanpa menulis apa pun.</summary>
        public const string ProductionEnvironmentName = "Production";

        public static async Task<InpatientMasterDataSeedResult> SeedAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            string environmentName,
            CancellationToken ct = default)
        {
            var result = new InpatientMasterDataSeedResult();

            if (IsProductionEnvironment(environmentName))
            {
                result.RefusedReason =
                    "Seeder master Rawat Inap menolak berjalan di lingkungan produksi. " +
                    "Isi pengaturan dan butir administrasi produksi lewat layar admin, " +
                    "bukan lewat seeder.";

                return result;
            }

            var now = DateTime.UtcNow;

            await SeedDefaultSettingAsync(db, actorUserId, now, result, ct);
            await SeedPrintSettingValuesAsync(db, actorUserId, now, result, ct);
            await SeedClearanceItemsAsync(db, actorUserId, now, result, ct);
            await SeedNewPatientHandoverItemsAsync(db, actorUserId, now, result, ct);

            return result;
        }

        /// <summary>
        /// Menentukan apakah nama lingkungan yang diberikan adalah produksi. Pembandingannya
        /// mengabaikan besar kecil huruf, mengikuti cara ASP.NET Core sendiri membaca
        /// ASPNETCORE_ENVIRONMENT.
        /// </summary>
        public static bool IsProductionEnvironment(string? environmentName)
            => string.Equals(
                environmentName?.Trim(),
                ProductionEnvironmentName,
                StringComparison.OrdinalIgnoreCase);

        // ------------------------------------------------------------------
        // Pengaturan Rawat Inap — satu baris berkode DEFAULT
        // ------------------------------------------------------------------

        /// <remarks>
        /// Nilai awal diambil dari 02-backend-architecture.md bagian 8.1.
        ///
        /// InitialAssessmentTargetHours dan ProgressNoteVerificationTargetHours bersumber dari
        /// RWI-RULE-021 yang BELUM final secara klinis. Keduanya di-seed sebagai nilai bawaan
        /// yang dapat diubah admin lewat layar pengaturan, dan tidak boleh diperlakukan sebagai
        /// angka yang sudah disahkan pemilik klinis.
        /// </remarks>
        private static async Task SeedDefaultSettingAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            InpatientMasterDataSeedResult result,
            CancellationToken ct)
        {
            // Idempotensi diperiksa lewat Code, kunci unik tabel ini. Pemeriksaan sengaja
            // tidak menyaring IsDelete: baris DEFAULT yang pernah dihapus lunak tetap
            // memakai Code yang sama di database, sehingga menambah baris kedua akan
            // menabrak index unik IX_MstInpatientSetting_Code dan menghentikan aplikasi
            // saat menyala.
            var alreadyExists = await db.Set<MstInpatientSetting>()
                .AnyAsync(x => x.Code == DefaultSettingCode, ct);

            if (alreadyExists)
            {
                result.SettingSkippedReason =
                    $"Baris pengaturan berkode {DefaultSettingCode} sudah ada, tidak ditambah lagi.";

                return;
            }

            db.Set<MstInpatientSetting>().Add(new MstInpatientSetting
            {
                Id = Guid.NewGuid(),
                Code = DefaultSettingCode,
                Name = "Pengaturan Rawat Inap Default",
                BedReservationMinutes = 120,
                DraftEpisodeExpiryHours = 24,
                InitialAssessmentTargetHours = 24,
                ProgressNoteVerificationTargetHours = 24,
                PendingClosureThresholdHours = 4,
                DepositFollowUpIntervalDays = 3,
                EpisodeNumberPrefix = "RI",
                IsDefault = true,
                IsActive = true,
                Notes =
                    "Nilai bawaan modul Rawat Inap. InitialAssessmentTargetHours dan " +
                    "ProgressNoteVerificationTargetHours bersumber dari RWI-RULE-021 yang " +
                    "belum final secara klinis; keduanya wajib ditinjau pemilik klinis " +
                    "sebelum dipakai untuk pasien sungguhan.",
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            await db.SaveChangesAsync(ct);
            result.SettingInserted = 1;
        }

        // ------------------------------------------------------------------
        // Butir administrasi penutupan episode
        // ------------------------------------------------------------------

        /// <remarks>
        /// Tiga butir bawaan sesuai RWI-DEC-026 dan 02-backend-architecture.md bagian 8.2.
        /// DISCHARGE-MED sengaja tidak wajib (RWI-RULE-024): obat pulang belum dapat ditutup
        /// otomatis karena modul Farmasi berada di luar scope MVP, sehingga menjadikannya
        /// wajib akan menahan penutupan setiap episode tanpa ada cara menyelesaikannya.
        /// </remarks>
        private static async Task SeedClearanceItemsAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            InpatientMasterDataSeedResult result,
            CancellationToken ct)
        {
            var definitions = new[]
            {
                new ClearanceItemDefinition(
                    "ADM-DOC",
                    "Berkas administrasi pasien lengkap",
                    "Berkas administrasi pasien sudah lengkap dan diserahkan ke bagian terkait.",
                    true,
                    10),
                new ClearanceItemDefinition(
                    "RETURN-ITEM",
                    "Barang milik pasien dan barang rumah sakit sudah diselesaikan",
                    "Barang milik pasien sudah dikembalikan dan barang milik rumah sakit sudah diterima kembali.",
                    true,
                    20),
                new ClearanceItemDefinition(
                    "DISCHARGE-MED",
                    "Obat pulang sudah diserahkan",
                    "Tidak wajib pada MVP karena modul Farmasi di luar scope. Dapat dinonaktifkan admin.",
                    false,
                    30)
            };

            // Sama seperti pengaturan: ItemCode adalah kunci unik tabel, dan pemeriksaannya
            // tidak menyaring IsDelete supaya baris yang pernah dihapus lunak tidak memicu
            // penyisipan kedua yang menabrak IX_MstInpatientClearanceItem_ItemCode.
            var existingCodes = await db.Set<MstInpatientClearanceItem>()
                .Select(x => x.ItemCode)
                .ToListAsync(ct);

            var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            foreach (var d in definitions)
            {
                if (existing.Contains(d.ItemCode))
                {
                    result.ClearanceItemSkipped++;
                    continue;
                }

                db.Set<MstInpatientClearanceItem>().Add(new MstInpatientClearanceItem
                {
                    Id = Guid.NewGuid(),
                    ItemCode = d.ItemCode,
                    ItemName = d.ItemName,
                    Description = d.Description,
                    IsMandatory = d.IsMandatory,
                    SortOrder = d.SortOrder,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.ClearanceItemInserted++;
            }

            if (result.ClearanceItemInserted > 0)
            {
                await db.SaveChangesAsync(ct);
            }
        }

        private sealed record ClearanceItemDefinition(
            string ItemCode,
            string ItemName,
            string Description,
            bool IsMandatory,
            int SortOrder);

        // ------------------------------------------------------------------
        // BE-RWI-186 — nilai cetak V1 pada baris pengaturan DEFAULT
        // ------------------------------------------------------------------

        /// <remarks>
        /// Nilai diambil dari <c>02-backend-architecture.md</c> 13.13 dan PRD Lampiran A.12. Hanya
        /// isian yang masih <b>kosong</b> yang diisi, sehingga kode yang sudah diganti admin tidak
        /// pernah tertimpa saat aplikasi dinyalakan ulang. Estimasi Biaya dan IPD sengaja tetap
        /// kosong karena V1 tidak punya kode formulir untuk keduanya; kode label rumah sakit tetap
        /// kosong supaya label memakai kode situs (G-36).
        /// </remarks>
        private static async Task SeedPrintSettingValuesAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            InpatientMasterDataSeedResult result,
            CancellationToken ct)
        {
            var setting = await db.Set<MstInpatientSetting>()
                .FirstOrDefaultAsync(x => x.Code == DefaultSettingCode && !x.IsDelete, ct);

            if (setting == null)
            {
                return;
            }

            var filled = 0;

            string? FillIfEmpty(string? current, string value)
            {
                if (!string.IsNullOrWhiteSpace(current))
                {
                    return current;
                }

                filled++;
                return value;
            }

            setting.GeneralConsentFormCode = FillIfEmpty(setting.GeneralConsentFormCode, "GC/ADM/001/Rev01/2024");
            setting.NewPatientHandoverFormCode = FillIfEmpty(setting.NewPatientHandoverFormCode, "HP/ADM/001/Rev01/2024");
            setting.DepositSettlementFormCode = FillIfEmpty(setting.DepositSettlementFormCode, "005/NM/E/Rev01/XI/2016");
            setting.CostDifferenceFormCode = FillIfEmpty(setting.CostDifferenceFormCode, "006/NM/E/Rev03/VI/2022");
            setting.PrivacyRequestFormCode = FillIfEmpty(setting.PrivacyRequestFormCode, "008/NM/E/Rev02/VI/2022");
            setting.BeliefValuesFormCode = FillIfEmpty(setting.BeliefValuesFormCode, "009/NM/E/Rev01/VI/2022");
            setting.DocumentSigningCity = FillIfEmpty(setting.DocumentSigningCity, "Jakarta");

            if (filled == 0)
            {
                return;
            }

            setting.UpdateDateTime = now;
            setting.UpdateBy = actorUserId;

            await db.SaveChangesAsync(ct);
            result.SettingPrintValuesFilled = filled;
        }

        // ------------------------------------------------------------------
        // BE-RWI-186 — butir Ceklist Serah Terima Pasien Baru (STPB-*)
        // ------------------------------------------------------------------

        /// <remarks>
        /// Lima belas butir dan tiga sub-butir V1 dengan nama mengikuti PRD Lampiran A.2
        /// (<c>02-backend-architecture.md</c> 13.13). Butir 12 ditulis "GENERAL CONSENT", bukan
        /// "GENERAL CONCERN" seperti V1. Idempoten per <c>ItemCode</c>, termasuk baris yang sudah
        /// dihapus lunak, sama seperti butir penutupan di atas.
        ///
        /// <para>
        /// Sumber saran tidak pernah dipasang dua kali: bila butir serah terima aktif lain sudah
        /// memakai sumber yang sama (misalnya dibuat admin lebih dulu), butir <c>STPB-*</c> masuk
        /// tanpa saran supaya aturan <c>MST-ICI-004</c> tetap utuh.
        /// </para>
        /// </remarks>
        private static async Task SeedNewPatientHandoverItemsAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            InpatientMasterDataSeedResult result,
            CancellationToken ct)
        {
            var definitions = new[]
            {
                new HandoverItemDefinition("STPB-01", "SURAT PENGANTAR RAWAT", null, MstHandoverSuggestionSource.ReferralLetter, 10),
                new HandoverItemDefinition("STPB-02", "MENGHUBUNGI DOKTER (KHUSUS TINDAKAN)", null, MstHandoverSuggestionSource.None, 20),
                new HandoverItemDefinition("STPB-02A", "HASIL PEMERIKSAAN PENUNJANG — Laboratorium", "STPB-02", MstHandoverSuggestionSource.None, 21),
                new HandoverItemDefinition("STPB-02B", "HASIL PEMERIKSAAN PENUNJANG — Radiologi", "STPB-02", MstHandoverSuggestionSource.None, 22),
                new HandoverItemDefinition("STPB-02C", "HASIL PEMERIKSAAN PENUNJANG — Lain-lain", "STPB-02", MstHandoverSuggestionSource.None, 23),
                new HandoverItemDefinition("STPB-03", "PENJELASAN DILARANG MEMBAWA OBAT DARI LUAR", null, MstHandoverSuggestionSource.None, 30),
                new HandoverItemDefinition("STPB-04", "PENJELASAN PRAKIRAAN BIAYA TINDAKAN & DEPOSIT", null, MstHandoverSuggestionSource.CostEstimateCompleted, 40),
                new HandoverItemDefinition("STPB-05", "PENJELASAN HARGA KAMAR", null, MstHandoverSuggestionSource.None, 50),
                new HandoverItemDefinition("STPB-06", "PENJELASAN TATA TERTIB", null, MstHandoverSuggestionSource.None, 60),
                new HandoverItemDefinition("STPB-07", "PERNYATAAN PELUNASAN DEPOSIT", null, MstHandoverSuggestionSource.DepositStatementCompleted, 70),
                new HandoverItemDefinition("STPB-08", "DOKUMEN MEDIK RI / RJ", null, MstHandoverSuggestionSource.None, 80),
                new HandoverItemDefinition("STPB-09", "FORMULIR IPD", null, MstHandoverSuggestionSource.BaseDataPrinted, 90),
                new HandoverItemDefinition("STPB-10", "FORMULIR ASURANSI / JAMINAN", null, MstHandoverSuggestionSource.None, 100),
                new HandoverItemDefinition("STPB-11", "INFORMASI VIP (KHUSUS VIP)", null, MstHandoverSuggestionSource.None, 110),
                new HandoverItemDefinition("STPB-12", "CETAK LABEL / STIKER / GENERAL CONSENT", null, MstHandoverSuggestionSource.LabelAndGeneralConsent, 120),
                new HandoverItemDefinition("STPB-13", "PASANG GELANG", null, MstHandoverSuggestionSource.WristbandPrinted, 130),
                new HandoverItemDefinition("STPB-14", "INPUT PARKIR", null, MstHandoverSuggestionSource.None, 140),
                new HandoverItemDefinition("STPB-15", "LAIN-LAIN", null, MstHandoverSuggestionSource.None, 150)
            };

            var existingRows = await db.Set<MstInpatientClearanceItem>()
                .Select(x => new
                {
                    x.Id,
                    x.ItemCode,
                    x.IsActive,
                    x.IsDelete,
                    x.ChecklistType,
                    x.HandoverSuggestionSource
                })
                .ToListAsync(ct);

            var idByCode = existingRows
                .GroupBy(x => x.ItemCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

            var takenSources = existingRows
                .Where(x => !x.IsDelete && x.IsActive &&
                    x.ChecklistType == MstClearanceChecklistType.NewPatientHandover &&
                    x.HandoverSuggestionSource != MstHandoverSuggestionSource.None)
                .Select(x => x.HandoverSuggestionSource)
                .ToHashSet();

            // Induk lebih dulu, baru sub-butir, supaya Id induk sudah diketahui saat sub-butir dibuat.
            foreach (var d in definitions.OrderBy(x => x.ParentCode == null ? 0 : 1))
            {
                if (idByCode.ContainsKey(d.ItemCode))
                {
                    result.HandoverItemSkipped++;
                    continue;
                }

                Guid? parentId = null;

                if (d.ParentCode != null)
                {
                    if (!idByCode.TryGetValue(d.ParentCode, out var resolvedParentId))
                    {
                        result.HandoverItemSkipped++;
                        continue;
                    }

                    parentId = resolvedParentId;
                }

                var source = d.SuggestionSource;

                if (source != MstHandoverSuggestionSource.None && !takenSources.Add(source))
                {
                    source = MstHandoverSuggestionSource.None;
                    result.HandoverSuggestionSourceDropped++;
                }

                var entity = new MstInpatientClearanceItem
                {
                    Id = Guid.NewGuid(),
                    ItemCode = d.ItemCode,
                    ItemName = d.ItemName,
                    Description = null,
                    IsMandatory = true,
                    SortOrder = d.SortOrder,
                    IsActive = true,
                    ChecklistType = MstClearanceChecklistType.NewPatientHandover,
                    ParentItemId = parentId,
                    HandoverSuggestionSource = source,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                db.Set<MstInpatientClearanceItem>().Add(entity);
                idByCode[d.ItemCode] = entity.Id;
                result.HandoverItemInserted++;
            }

            if (result.HandoverItemInserted > 0)
            {
                await db.SaveChangesAsync(ct);
            }
        }

        private sealed record HandoverItemDefinition(
            string ItemCode,
            string ItemName,
            string? ParentCode,
            MstHandoverSuggestionSource SuggestionSource,
            int SortOrder);
    }

    /// <summary>
    /// Ringkasan hasil seeder, dipakai untuk pencatatan log agar terlihat berapa baris yang
    /// benar-benar ditambahkan dan bagian mana yang dilewati beserta alasannya.
    /// </summary>
    public class InpatientMasterDataSeedResult
    {
        /// <summary>
        /// Terisi hanya bila seeder menolak berjalan, misalnya di lingkungan produksi.
        /// </summary>
        public string? RefusedReason { get; set; }

        public bool Refused => !string.IsNullOrWhiteSpace(RefusedReason);

        public int SettingInserted { get; set; }

        public string? SettingSkippedReason { get; set; }

        public int ClearanceItemInserted { get; set; }

        public int ClearanceItemSkipped { get; set; }

        /// <summary>Isian cetak V1 yang diisi karena masih kosong (<c>BE-RWI-186</c>).</summary>
        public int SettingPrintValuesFilled { get; set; }

        /// <summary>Butir serah terima <c>STPB-*</c> yang ditambahkan (<c>BE-RWI-186</c>).</summary>
        public int HandoverItemInserted { get; set; }

        /// <summary>Butir serah terima yang dilewati karena kodenya sudah ada.</summary>
        public int HandoverItemSkipped { get; set; }

        /// <summary>Butir serah terima yang masuk tanpa saran karena sumbernya sudah dipakai butir lain.</summary>
        public int HandoverSuggestionSourceDropped { get; set; }

        public int TotalInserted => SettingInserted + ClearanceItemInserted + HandoverItemInserted;
    }
}
