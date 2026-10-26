namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Keadaan batch kanonik yang disetujui pemilik untuk worker migrasi massal RSMMC
    /// (`BE-PAT-MIG-002`) — angka gerbang tingkat panggilan kontrak `1.3.0` B4.2.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Seluruh angka adalah konstanta yang disetujui, bukan konfigurasi dan bukan masukan request,
    /// sehingga tidak dapat dilonggarkan saat runtime. Batch lain tidak punya ekspektasi yang
    /// disetujui, sehingga eksekusinya ditolak; simulasi tetap melaporkan hasil gerbangnya.
    /// </para>
    /// <para>
    /// <see cref="ApprovedPatientBaseline"/> = 741 berasal dari pengukuran read-only pemilik
    /// 22 Oktober 2026 sesudah `PAT-GATE-005` generasi 2 `PASS` (`PAT-DEC-077`, `PAT-FACT-095`), bukan
    /// dari angka historis `PAT-FACT-038` dan bukan dari jumlah yang ada saat eksekusi.
    /// </para>
    /// </remarks>
    public sealed record RsmmcBulkApprovedBatchProfile(
        Guid BatchId,
        Guid PilotBatchId,
        long ExpectedPreviewRows,
        long ExpectedFinalizedPlannerRows,
        IReadOnlyList<long> DeferredLegacyPids,
        IReadOnlyList<string> DeferredReservedPatientCodes,
        long ApprovedPatientBaseline,
        long ExpectedPilotTargets,
        long ExpectedPilotAutoMigrated,
        long ExpectedPilotManualMigrated,
        long ExpectedPilotPreviewRows,
        long ExpectedBackupRows)
    {
        /// <summary>
        /// key1 kunci transaksi dua bagian per pasien: 32 bit pertama UUID batch kanonik,
        /// <c>0x45EEEFBA</c> = 1.173.286.842, dicadangkan untuk worker ini (`PAT-DEC-048`, kontrak B4.3).
        /// Berbeda dari kunci allocator <c>(1346458697, 1313821261)</c> milik `BE-PAT-MIG-003`.
        /// </summary>
        public const int WorkerLockNamespace = 0x45EEEFBA;

        /// <summary>Subfolder namespace QR bulk di bawah <c>patient-qrcodes</c> (`PAT-DEC-052`).</summary>
        public const string QrNamespaceFolderName = "rsmmc-bulk";

        /// <summary><c>match_status</c> baris crosswalk baru (`PAT-DEC-051`).</summary>
        public const string MatchStatusTargetCreated = "TARGET_CREATED";

        /// <summary><c>migration_status</c> baris crosswalk baru — provenance worker (`PAT-DEC-034`).</summary>
        public const string MigrationStatusBulkWorker = "MIGRATED_BULK_WORKER";

        /// <summary><c>migration_status</c> historis Pilot (`PAT-FACT-063`). Tidak pernah ditulis worker.</summary>
        public const string MigrationStatusPilotAuto = "MIGRATED_API_AUTO";

        /// <summary><c>migration_status</c> historis Pilot (`PAT-FACT-063`). Tidak pernah ditulis worker.</summary>
        public const string MigrationStatusPilotManual = "MIGRATED_API_PILOT";

        public const string PreviewPendingStatus = "PENDING";

        public const string PlannerFinalizedStatus = "FINALIZED";

        /// <summary>
        /// Batch kanonik `PAT-DEC-004` beserta angka yang dilaporkan pemilik: preview 704.506
        /// (`PAT-FACT-040`), perencana `FINALIZED` 705.223 (`PAT-FACT-084`), tertunda 280143 dan 805596
        /// dengan kode cadangannya (`PAT-FACT-037`, `043`), baseline 741 (`PAT-FACT-095`), pemetaan Pilot
        /// 715 = 710 + 5 (`PAT-FACT-060`–`063`), preview Pilot 715 (`PAT-FACT-049`), cadangan 715.
        /// </summary>
        public static RsmmcBulkApprovedBatchProfile Canonical { get; } = new(
            BatchId: Guid.Parse("45eeefba-dd39-6831-1bd2-d986851f7c1f"),
            PilotBatchId: Guid.Parse("2ce3929b-b5af-430b-a2b4-bdb3aaf6908f"),
            ExpectedPreviewRows: 704506,
            ExpectedFinalizedPlannerRows: 705223,
            DeferredLegacyPids: [280143, 805596],
            DeferredReservedPatientCodes: ["PAT-RSMMC-101416", "PAT-RSMMC-619077"],
            ApprovedPatientBaseline: 741,
            ExpectedPilotTargets: 715,
            ExpectedPilotAutoMigrated: 710,
            ExpectedPilotManualMigrated: 5,
            ExpectedPilotPreviewRows: 715,
            ExpectedBackupRows: 715);

        /// <summary>
        /// Mengubah <c>legacy_pid</c> Int64 menjadi key2 kunci Int32 dengan pemeriksaan rentang eksplisit.
        /// Nilai negatif sah selama berada di rentang Int32; di luar itu ditolak (`LOCK_KEY_OUT_OF_RANGE`).
        /// </summary>
        public static bool TryConvertLockKey(long legacyPid, out int lockKey)
        {
            if (legacyPid < int.MinValue || legacyPid > int.MaxValue)
            {
                lockKey = 0;
                return false;
            }

            lockKey = (int)legacyPid;
            return true;
        }
    }
}
