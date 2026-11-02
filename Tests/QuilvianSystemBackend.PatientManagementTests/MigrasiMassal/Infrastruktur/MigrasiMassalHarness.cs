using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal.Infrastruktur
{
    public sealed record BarisCrosswalkUji(
        long LegacyPid,
        long? CanonicalLegacyPid,
        string? LegacyOldRm,
        string? NormalizedMrn,
        string? NormalizedNik,
        Guid? QuilvianPatientId,
        string MatchStatus,
        string MigrationStatus,
        string? ReviewReason,
        Guid? LastBatchId,
        string? CreatedAt,
        string? UpdatedAt);

    /// <summary>
    /// Basis data uji SQLite dengan skema <c>migration</c> tiruan, folder QR sementara, dan service
    /// migrasi massal produksi (`BE-PAT-MIG-002`).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Data baku: 25 pasien manifest dengan sebaran kelompok sumber 7/6/6/6, tiga baris preview kanonik di
    /// luar manifest, tiga pasien Pilot beserta pemetaan, preview Pilot, dan cadangannya, dua
    /// <c>legacy_pid</c> tertunda, satu crosswalk historis tanpa target, dan dua QR historis di path bawaan.
    /// Jumlah pada profil uji diturunkan <b>sekali</b> dari data baku — seperti nilai yang disetujui pemilik
    /// — dan tidak dihitung ulang saat service dibuat, sehingga perubahan data sesudahnya terlihat oleh gerbang.
    /// </para>
    /// <para>
    /// Uji tidak pernah menyentuh basis data mana pun yang tercatat di appsettings. Folder QR berada di
    /// direktori sementara sistem operasi dan dihapus pada <see cref="Dispose"/>.
    /// </para>
    /// </remarks>
    public sealed class MigrasiMassalHarness : IDisposable
    {
        public static readonly Guid BatchKanonik = RsmmcBulkApprovedBatchProfile.Canonical.BatchId;
        public static readonly Guid BatchPilot = RsmmcBulkApprovedBatchProfile.Canonical.PilotBatchId;
        public static readonly Guid AktorUji = Guid.Parse("0b5e7a10-0000-4000-8000-00000000e002");

        public const string PublicRequestPath = "/uploads";

        public static readonly long[] PidPilot = [9001, 9002, 9003];
        public static readonly long[] PidDiLuarManifest = [500001, 500002, 500003];
        public const long PidCrosswalkTanpaTarget = 600001;

        /// <summary>Kelompok sumber pasien ke-<c>i</c> adalah <c>KelompokSumber[i % 4]</c> — 25 pasien → 7/6/6/6.</summary>
        public static readonly string[] KelompokSumber = ["NOREG_WITHOUT_RM", "NOREG_WITH_RM", "REG_WITHOUT_RM", "REG_WITH_RM"];

        public static readonly string[] MrnTertunda = ["00-88-00-01", "00-88-00-02"];

        private static readonly DateTime WaktuDibuat = new(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);
        private static readonly Guid PembuatAwal = Guid.Parse("0b5e7a10-0000-4000-8000-00000000c0de");

        private readonly List<ApplicationDbContext> _konteks = [];
        private readonly DbConnection _koneksi;

        public MigrasiMassalHarness(RsmmcBulkCanaryManifest? manifest = null)
        {
            Manifest = manifest ?? RsmmcBulkCanaryManifest.CanaryAV1;
            Database = TestDatabase.Create();
            StorageRoot = Path.Combine(Path.GetTempPath(), "quilvian-pat-mig2-uji", Guid.NewGuid().ToString("N"));

            // Koneksi SQLite milik TestDatabase tetap terbuka sepanjang uji; konteks tidak menutupnya.
            using (var konteks = Database.CreateContext())
            {
                _koneksi = konteks.Database.GetDbConnection();
            }

            BentukSkemaMigrasi();
            SiapkanDataBaku();
            Profil = ProfilDariDataBaku();
        }

        public TestDatabase Database { get; }

        public RsmmcBulkCanaryManifest Manifest { get; }

        public RsmmcBulkApprovedBatchProfile Profil { get; set; }

        public KendaliStoreUji Kendali { get; } = new();

        public PencatatLogUji Log { get; } = new();

        public PenghitungPerintahDb Perintah { get; } = new();

        public PenghitungTransaksi Transaksi { get; } = new();

        public string EnvironmentName { get; set; } = "Staging";

        public KonfigurasiGuardUji KonfigurasiGuard { get; set; } = KonfigurasiGuardUji.EnabledTrue;

        public string StorageRoot { get; }

        /// <summary>Berapa kali penulis QR meminta folder penyimpanan — yang membuat folder.</summary>
        public int JumlahResolveStorage { get; private set; }

        public List<string> PayloadDirender { get; } = [];

        /// <summary>Dipanggil saat PNG dibentuk — sesudah pemeriksaan awal, sebelum penulisan akhir.</summary>
        public Action<string>? SaatRender { get; set; }

        public bool GagalRender { get; set; }

        // ---- Penamaan data baku --------------------------------------------------------------

        public long PidManifest(int urutan) => Manifest.LegacyPids[urutan];

        public static string KodeUntuk(int nomor) => $"PAT-RSMMC-{100000 + nomor}";

        public static string MrnUntuk(int nomor) => $"00-12-{nomor / 100:00}-{nomor % 100:00}";

        public static string NamaUntuk(int nomor) => $"Nama Rahasia Pasien {nomor:000}";

        public static string NikUntuk(int nomor) => $"3171010101{nomor:000000}";

        public static string TeleponUntuk(int nomor) => $"0812555{nomor:00000}";

        public static string AlamatUntuk(int nomor) => $"Alamat Rahasia Jalan {nomor:000}";

        public static DateTime TanggalLahirUntuk(int nomor) => new(1950 + nomor % 50, 1 + nomor % 12, 1 + nomor % 28);

        public static string PathQrBulkPublik(string mrn) => $"/uploads/patient-qrcodes/rsmmc-bulk/{mrn}/qrcode.png";

        public string PathQrBulkFisik(string mrn) => Path.Combine(StorageRoot, "patient-qrcodes", "rsmmc-bulk", mrn, "qrcode.png");

        public string PathQrHistorisFisik(string mrn) => Path.Combine(StorageRoot, "patient-qrcodes", mrn, "qrcode.png");

        public string FolderNamespaceBulk => Path.Combine(StorageRoot, "patient-qrcodes", "rsmmc-bulk");

        /// <summary>Baris preview kanonik pasien manifest ke-<paramref name="urutan"/> persis seperti disemai.</summary>
        public RsmmcBulkPreviewRow PreviewManifest(int urutan) => BuatPreview(BatchKanonik, PidManifest(urutan), urutan);

        public static RsmmcBulkPreviewRow BuatPreview(Guid batchId, long legacyPid, int nomor)
        {
            var denganRm = nomor % 4 is 1 or 3;

            return new RsmmcBulkPreviewRow
            {
                BatchId = batchId,
                LegacyPid = legacyPid,
                SourceGroup = KelompokSumber[nomor % 4],
                LegacyOldRm = denganRm ? $"RM{nomor:000000}" : null,
                LegacyNormalizedMrn = denganRm ? $"77-77-{nomor / 100:00}-{nomor % 100:00}" : null,
                LegacyNormalizedNik = nomor % 3 == 0 ? $"3171990000{nomor:000000}" : null,
                ProposedPatientCode = KodeUntuk(nomor),
                ProposedMedicalRecordNumber = MrnUntuk(nomor),
                PatientType = (int)PatientType.General,
                PatientStatus = (int)PatientStatus.Active,
                RegistrationSource = (int)PatientRegistrationSource.Migration,
                Gender = nomor % 3 == 2 ? null : nomor % 2 == 0 ? 1 : 2,
                Religion = 1,
                MaritalStatus = nomor % 2 == 0 ? 1 : 2,
                BloodType = 0,
                FullName = NamaUntuk(nomor),
                BirthDate = TanggalLahirUntuk(nomor),
                IdentityType = "KTP",
                IdentityNumber = nomor % 4 == 3 ? null : NikUntuk(nomor),
                PhoneNumber = nomor % 5 == 4 ? null : TeleponUntuk(nomor),
                Address = AlamatUntuk(nomor),
                IsMember = false,
                IsNewborn = false,
                IsDeceased = nomor == 7,
                IsActive = true,
                ValidationStatus = RsmmcBulkApprovedBatchProfile.PreviewPendingStatus
            };
        }

        /// <summary>Manifest uji dengan sidik jari dan sebaran kelompok yang dihitung dari isinya sendiri.</summary>
        public static RsmmcBulkCanaryManifest ManifestUji(params long[] legacyPids) =>
            new(
                "UJI-MANIFEST",
                1,
                legacyPids,
                RsmmcBulkCanaryManifest.ComputeSha256(RsmmcBulkCanaryManifest.BuildCanonicalText(legacyPids)),
                legacyPids
                    .Select((_, i) => KelompokSumber[i % 4])
                    .GroupBy(x => x, StringComparer.Ordinal)
                    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal));

        // ---- Service --------------------------------------------------------------------------

        public RsmmcBulkPatientMigrationService BuatService(params IInterceptor[] interceptors)
        {
            var konteks = Database.CreateContext([Perintah, Transaksi, .. interceptors]);
            _konteks.Add(konteks);

            var lingkungan = new LingkunganUji(EnvironmentName);

            return new RsmmcBulkPatientMigrationService(
                konteks,
                new TiruanStoreMigrasiMassal(konteks, Kendali),
                lingkungan,
                new LoggerService(Log, new HttpContextAccessor()),
                Profil,
                Manifest,
                new PatientIdentifierAllocationService(konteks, lingkungan, ReservasiHarness.BuatKonfigurasi(KonfigurasiGuard)));
        }

        public Task<RsmmcBulkPatientMigrationResult> JalankanAsync(
            bool? dryRun,
            int? limit = null,
            CancellationToken cancellationToken = default,
            params IInterceptor[] interceptors) =>
            JalankanPermintaanAsync(
                new RsmmcBulkPatientMigrationRequest
                {
                    BatchId = BatchKanonik,
                    SelectionMode = RsmmcBulkSelectionMode.CanaryA,
                    DryRun = dryRun,
                    Limit = limit
                },
                AktorUji,
                cancellationToken,
                interceptors);

        public Task<RsmmcBulkPatientMigrationResult> JalankanPermintaanAsync(
            RsmmcBulkPatientMigrationRequest permintaan,
            Guid aktor,
            CancellationToken cancellationToken = default,
            params IInterceptor[] interceptors) =>
            BuatService(interceptors).MigrateAsync(permintaan, aktor, BuatStoreQr(), BuatProbe(), "uji-trace-mig2", cancellationToken);

        /// <summary>
        /// Penulis QR produksi dengan layout namespace <c>rsmmc-bulk</c>, seperti
        /// <c>PatientController.CreateRsmmcBulkQrArtifactStore</c>. PNG diganti teks supaya isi mudah diperiksa.
        /// </summary>
        public RsmmcPilotQrArtifactStore BuatStoreQr() =>
            new(
                new RsmmcPilotQrArtifactLayout(PublicRequestPath, Path.Combine("patient-qrcodes", "rsmmc-bulk"), "qrcode.png"),
                () =>
                {
                    // Sama dengan GetFileStoragePaths: meminta lokasi penyimpanan membuat foldernya.
                    JumlahResolveStorage++;
                    Directory.CreateDirectory(StorageRoot);
                    return (StorageRoot, null);
                },
                new RsmmcPilotQrArtifactFormat(
                    FormatQrPasien.Sanitize,
                    FormatQrPasien.Payload,
                    (payload, _, _) =>
                    {
                        PayloadDirender.Add(payload);
                        SaatRender?.Invoke(payload);

                        if (GagalRender)
                        {
                            throw new InvalidOperationException("Simulasi kegagalan membentuk PNG QR.");
                        }

                        return Encoding.UTF8.GetBytes("QR-BARU:" + payload);
                    },
                    FormatQrPasien.Combine));

        public RsmmcQrDestinationProbe BuatProbe() =>
            new(
                () => StorageRoot,
                new RsmmcPilotQrArtifactLayout(PublicRequestPath, Path.Combine("patient-qrcodes", "rsmmc-bulk"), "qrcode.png"),
                new RsmmcPilotQrArtifactLayout(PublicRequestPath, "patient-qrcodes", "qrcode.png"),
                FormatQrPasien.Sanitize);

        // ---- Mutasi data uji ------------------------------------------------------------------

        public Guid TambahPasien(
            string patientCode,
            string medicalRecordNumber,
            string? identityNumber = null,
            string? phoneNumber = null,
            bool terhapus = false,
            string? qrCodePath = null,
            Guid? id = null,
            DbTransaction? transaksi = null)
        {
            var pasienId = id ?? Guid.NewGuid();

            using var konteks = Database.CreateContext();

            if (transaksi is not null)
            {
                konteks.Database.UseTransaction(transaksi);
            }

            konteks.Set<MstPatient>().Add(new MstPatient
            {
                Id = pasienId,
                PatientCode = patientCode,
                MedicalRecordNumber = medicalRecordNumber,
                FullName = "Pasien Lama Uji",
                IdentityNumber = identityNumber,
                PhoneNumber = phoneNumber,
                QrCodePath = qrCodePath,
                IsDelete = terhapus,
                DeleteDateTime = terhapus ? WaktuDibuat : null,
                CreateDateTime = WaktuDibuat,
                CreateBy = PembuatAwal
            });

            konteks.SaveChanges();
            return pasienId;
        }

        public void TambahPreview(RsmmcBulkPreviewRow p) =>
            Jalankan(
                "INSERT INTO migration.rsmmc_patient_target_preview " +
                "(batch_id, legacy_pid, source_group, legacy_old_rm, legacy_normalized_mrn, legacy_normalized_nik, " +
                "proposed_patient_code, proposed_medical_record_number, patient_type, patient_status, registration_source, " +
                "gender, religion, marital_status, blood_type, full_name, birth_date, identity_type, identity_number, " +
                "phone_number, address, is_member, is_newborn, is_deceased, is_active, validation_status) VALUES " +
                "($batch_id, $legacy_pid, $source_group, $legacy_old_rm, $legacy_normalized_mrn, $legacy_normalized_nik, " +
                "$proposed_patient_code, $proposed_medical_record_number, $patient_type, $patient_status, $registration_source, " +
                "$gender, $religion, $marital_status, $blood_type, $full_name, $birth_date, $identity_type, $identity_number, " +
                "$phone_number, $address, $is_member, $is_newborn, $is_deceased, $is_active, $validation_status)",
                ("$batch_id", Teks(p.BatchId)),
                ("$legacy_pid", p.LegacyPid),
                ("$source_group", p.SourceGroup),
                ("$legacy_old_rm", p.LegacyOldRm),
                ("$legacy_normalized_mrn", p.LegacyNormalizedMrn),
                ("$legacy_normalized_nik", p.LegacyNormalizedNik),
                ("$proposed_patient_code", p.ProposedPatientCode),
                ("$proposed_medical_record_number", p.ProposedMedicalRecordNumber),
                ("$patient_type", p.PatientType),
                ("$patient_status", p.PatientStatus),
                ("$registration_source", p.RegistrationSource),
                ("$gender", p.Gender),
                ("$religion", p.Religion),
                ("$marital_status", p.MaritalStatus),
                ("$blood_type", p.BloodType),
                ("$full_name", p.FullName),
                ("$birth_date", p.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                ("$identity_type", p.IdentityType),
                ("$identity_number", p.IdentityNumber),
                ("$phone_number", p.PhoneNumber),
                ("$address", p.Address),
                ("$is_member", Bit(p.IsMember)),
                ("$is_newborn", Bit(p.IsNewborn)),
                ("$is_deceased", Bit(p.IsDeceased)),
                ("$is_active", Bit(p.IsActive)),
                ("$validation_status", p.ValidationStatus));

        /// <summary>Mengubah satu kolom preview batch kanonik untuk satu <c>legacy_pid</c>.</summary>
        public void UbahPreview(long legacyPid, string kolom, object? nilai) =>
            Jalankan(
                $"UPDATE migration.rsmmc_patient_target_preview SET {kolom} = $nilai WHERE batch_id = $batch_id AND legacy_pid = $legacy_pid",
                ("$nilai", nilai),
                ("$batch_id", Teks(BatchKanonik)),
                ("$legacy_pid", legacyPid));

        public void TambahPerencana(long legacyPid, string? finalMedicalRecordNumber, string planStatus = "FINALIZED", Guid? batchId = null) =>
            Jalankan(
                "INSERT INTO migration.rsmmc_patient_mrn_plan (batch_id, legacy_pid, final_medical_record_number, plan_status) " +
                "VALUES ($batch_id, $legacy_pid, $final, $status)",
                ("$batch_id", Teks(batchId ?? BatchKanonik)),
                ("$legacy_pid", legacyPid),
                ("$final", finalMedicalRecordNumber),
                ("$status", planStatus));

        public void UbahPerencana(long legacyPid, string kolom, object? nilai) =>
            Jalankan(
                $"UPDATE migration.rsmmc_patient_mrn_plan SET {kolom} = $nilai WHERE batch_id = $batch_id AND legacy_pid = $legacy_pid",
                ("$nilai", nilai),
                ("$batch_id", Teks(BatchKanonik)),
                ("$legacy_pid", legacyPid));

        public void TambahCrosswalk(
            long legacyPid,
            Guid? target,
            string matchStatus,
            string migrationStatus,
            Guid? lastBatchId,
            DbTransaction? transaksi = null) =>
            JalankanDalam(
                transaksi,
                "INSERT INTO migration.rsmmc_patient_crosswalk " +
                "(legacy_pid, quilvian_patient_id, match_status, migration_status, last_batch_id, created_at) " +
                "VALUES ($legacy_pid, $target, $match, $migration, $batch, '2026-09-01 08:00:00')",
                ("$legacy_pid", legacyPid),
                ("$target", target is { } t ? Teks(t) : null),
                ("$match", matchStatus),
                ("$migration", migrationStatus),
                ("$batch", lastBatchId is { } b ? Teks(b) : null));

        public void Jalankan(string sql, params (string Nama, object? Nilai)[] parameter) => JalankanDalam(null, sql, parameter);

        public void JalankanDalam(DbTransaction? transaksi, string sql, params (string Nama, object? Nilai)[] parameter)
        {
            using var perintah = _koneksi.CreateCommand();
            perintah.Transaction = transaksi;
            perintah.CommandText = sql;

            foreach (var (nama, nilai) in parameter)
            {
                var p = perintah.CreateParameter();
                p.ParameterName = nama;
                p.Value = nilai ?? DBNull.Value;
                perintah.Parameters.Add(p);
            }

            perintah.ExecuteNonQuery();
        }

        public static void TulisFile(string path, string isi)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, isi);
        }

        // ---- Pembacaan ------------------------------------------------------------------------

        public int JumlahPasien()
        {
            using var konteks = Database.CreateContext();
            return konteks.Set<MstPatient>().IgnoreQueryFilters().Count();
        }

        public MstPatient? PasienDenganKode(string patientCode)
        {
            using var konteks = Database.CreateContext();

            return konteks.Set<MstPatient>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleOrDefault(x => x.PatientCode == patientCode);
        }

        public List<BarisCrosswalkUji> BacaCrosswalk()
        {
            using var perintah = _koneksi.CreateCommand();
            perintah.CommandText =
                "SELECT legacy_pid, canonical_legacy_pid, legacy_old_rm, normalized_mrn, normalized_nik, quilvian_patient_id, " +
                "match_status, migration_status, review_reason, last_batch_id, created_at, updated_at " +
                "FROM migration.rsmmc_patient_crosswalk ORDER BY legacy_pid";

            using var r = perintah.ExecuteReader();
            var hasil = new List<BarisCrosswalkUji>();

            while (r.Read())
            {
                hasil.Add(new BarisCrosswalkUji(
                    r.GetInt64(0),
                    r.IsDBNull(1) ? null : r.GetInt64(1),
                    r.IsDBNull(2) ? null : r.GetString(2),
                    r.IsDBNull(3) ? null : r.GetString(3),
                    r.IsDBNull(4) ? null : r.GetString(4),
                    r.IsDBNull(5) ? null : Guid.Parse(r.GetString(5)),
                    r.GetString(6),
                    r.GetString(7),
                    r.IsDBNull(8) ? null : r.GetString(8),
                    r.IsDBNull(9) ? null : Guid.Parse(r.GetString(9)),
                    r.IsDBNull(10) ? null : r.GetString(10),
                    r.IsDBNull(11) ? null : r.GetString(11)));
            }

            return hasil;
        }

        public BarisCrosswalkUji? CrosswalkUntuk(long legacyPid) => BacaCrosswalk().SingleOrDefault(x => x.LegacyPid == legacyPid);

        /// <summary>Seluruh isi <c>MstPatient</c> dan kelima tabel migrasi sebagai teks, untuk pembanding sebelum–sesudah.</summary>
        public string SnapshotDatabase()
        {
            var hasil = new StringBuilder();

            using (var konteks = Database.CreateContext())
            {
                var properti = typeof(MstPatient)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && IsSkalar(p.PropertyType))
                    .OrderBy(p => p.Name, StringComparer.Ordinal)
                    .ToList();

                foreach (var pasien in konteks.Set<MstPatient>().IgnoreQueryFilters().AsNoTracking().OrderBy(x => x.Id).ToList())
                {
                    hasil.AppendLine("MstPatient|" + string.Join("|", properti.Select(p => $"{p.Name}={p.GetValue(pasien)}")));
                }
            }

            foreach (var tabel in new[]
            {
                "rsmmc_patient_target_preview", "rsmmc_patient_mrn_plan", "rsmmc_patient_crosswalk",
                "rsmmc_patient_batch", "rsmmc_patient_pilot_reconcile_backup"
            })
            {
                using var perintah = _koneksi.CreateCommand();
                perintah.CommandText = $"SELECT * FROM migration.{tabel}";

                using var r = perintah.ExecuteReader();
                var baris = new List<string>();

                while (r.Read())
                {
                    var nilai = Enumerable.Range(0, r.FieldCount).Select(i => r.IsDBNull(i) ? "NULL" : Convert.ToString(r.GetValue(i), CultureInfo.InvariantCulture));
                    baris.Add(tabel + "|" + string.Join("|", nilai));
                }

                foreach (var b in baris.OrderBy(x => x, StringComparer.Ordinal))
                {
                    hasil.AppendLine(b);
                }
            }

            return hasil.ToString();
        }

        /// <summary>Seluruh folder dan berkas di bawah folder penyimpanan sementara beserta isinya.</summary>
        public SortedDictionary<string, string> SnapshotFilesystem()
        {
            var hasil = new SortedDictionary<string, string>(StringComparer.Ordinal);

            if (!Directory.Exists(StorageRoot))
            {
                return hasil;
            }

            foreach (var folder in Directory.EnumerateDirectories(StorageRoot, "*", SearchOption.AllDirectories))
            {
                hasil["[dir] " + Path.GetRelativePath(StorageRoot, folder)] = string.Empty;
            }

            foreach (var berkas in Directory.EnumerateFiles(StorageRoot, "*", SearchOption.AllDirectories))
            {
                hasil[Path.GetRelativePath(StorageRoot, berkas)] = File.ReadAllText(berkas);
            }

            return hasil;
        }

        public void Dispose()
        {
            foreach (var konteks in _konteks)
            {
                konteks.Dispose();
            }

            Database.Dispose();

            try
            {
                if (Directory.Exists(StorageRoot))
                {
                    Directory.Delete(StorageRoot, recursive: true);
                }
            }
            catch (IOException)
            {
                // Folder sementara milik uji; sisa penguncian berkas tidak boleh menggagalkan uji.
            }
        }

        // ---- Pembentukan data baku -------------------------------------------------------------

        private void BentukSkemaMigrasi()
        {
            Jalankan("ATTACH DATABASE ':memory:' AS migration");

            Jalankan(
                "CREATE TABLE migration.rsmmc_patient_target_preview (" +
                "batch_id TEXT NOT NULL, legacy_pid INTEGER NOT NULL, source_group TEXT NOT NULL, " +
                "legacy_old_rm TEXT, legacy_normalized_mrn TEXT, legacy_normalized_nik TEXT, " +
                "proposed_patient_code TEXT NOT NULL, proposed_medical_record_number TEXT NOT NULL, " +
                "patient_type INTEGER, patient_status INTEGER, registration_source INTEGER, gender INTEGER, " +
                "religion INTEGER, marital_status INTEGER, blood_type INTEGER, full_name TEXT, birth_date TEXT, " +
                "identity_type TEXT, identity_number TEXT, phone_number TEXT, address TEXT, " +
                "is_member INTEGER, is_newborn INTEGER, is_deceased INTEGER, is_active INTEGER, " +
                "validation_status TEXT, PRIMARY KEY (batch_id, legacy_pid))");

            Jalankan(
                "CREATE TABLE migration.rsmmc_patient_mrn_plan (" +
                "batch_id TEXT NOT NULL, legacy_pid INTEGER NOT NULL, final_medical_record_number TEXT, " +
                "plan_status TEXT NOT NULL, PRIMARY KEY (batch_id, legacy_pid))");

            Jalankan(
                "CREATE TABLE migration.rsmmc_patient_crosswalk (" +
                "legacy_pid INTEGER NOT NULL PRIMARY KEY, canonical_legacy_pid INTEGER, legacy_old_rm TEXT, " +
                "normalized_mrn TEXT, normalized_nik TEXT, quilvian_patient_id TEXT, match_status TEXT NOT NULL, " +
                "migration_status TEXT NOT NULL, review_reason TEXT, last_batch_id TEXT, created_at TEXT NOT NULL, updated_at TEXT)");

            Jalankan("CREATE TABLE migration.rsmmc_patient_batch (batch_id TEXT NOT NULL PRIMARY KEY)");

            Jalankan(
                "CREATE TABLE migration.rsmmc_patient_pilot_reconcile_backup (" +
                "batch_id TEXT NOT NULL, legacy_pid INTEGER NOT NULL)");
        }

        private void SiapkanDataBaku()
        {
            Jalankan("INSERT INTO migration.rsmmc_patient_batch (batch_id) VALUES ($b)", ("$b", Teks(BatchKanonik)));

            // Pasien yang sudah ada sebelum worker: dua pasien biasa dan tiga pasien Pilot.
            TambahPasien("PAT-RSMMC-00001", "00-00-00-01");
            TambahPasien("PAT-RSMMC-00002", "00-00-00-02");

            for (var i = 0; i < PidPilot.Length; i++)
            {
                var mrn = $"00-79-70-{10 + i:00}";
                var id = TambahPasien($"PAT-RSMMC-{901 + i:00000}", mrn, qrCodePath: $"/uploads/patient-qrcodes/{mrn}/qrcode.png");

                TambahPerencana(PidPilot[i], mrn);
                TambahCrosswalk(
                    PidPilot[i],
                    id,
                    RsmmcBulkApprovedBatchProfile.MatchStatusTargetCreated,
                    i < 2 ? RsmmcBulkApprovedBatchProfile.MigrationStatusPilotAuto : RsmmcBulkApprovedBatchProfile.MigrationStatusPilotManual,
                    BatchKanonik);
                TambahPreview(BuatPreview(BatchPilot, PidPilot[i], 900 + i));
                Jalankan(
                    "INSERT INTO migration.rsmmc_patient_pilot_reconcile_backup (batch_id, legacy_pid) VALUES ($b, $p)",
                    ("$b", Teks(BatchKanonik)),
                    ("$p", PidPilot[i]));
            }

            // Salah satu dari 285 crosswalk historis tanpa target, di luar manifest.
            TambahCrosswalk(PidCrosswalkTanpaTarget, null, "UNPROCESSED", "RAW", null);

            var disemai = new HashSet<long>();

            for (var i = 0; i < Manifest.LegacyPids.Count; i++)
            {
                var legacyPid = Manifest.LegacyPids[i];

                // legacy_pid tertunda tidak pernah punya preview kanonik; manifest uji bernilai ganda disemai sekali.
                if (RsmmcBulkApprovedBatchProfile.Canonical.DeferredLegacyPids.Contains(legacyPid) || !disemai.Add(legacyPid))
                {
                    continue;
                }

                var preview = BuatPreview(BatchKanonik, legacyPid, i);
                TambahPreview(preview);
                TambahPerencana(legacyPid, preview.ProposedMedicalRecordNumber);
            }

            for (var j = 0; j < PidDiLuarManifest.Length; j++)
            {
                var preview = BuatPreview(BatchKanonik, PidDiLuarManifest[j], 100 + j);
                TambahPreview(preview);
                TambahPerencana(PidDiLuarManifest[j], preview.ProposedMedicalRecordNumber);
            }

            var tertunda = RsmmcBulkApprovedBatchProfile.Canonical.DeferredLegacyPids;

            for (var k = 0; k < tertunda.Count; k++)
            {
                TambahPerencana(tertunda[k], MrnTertunda[k]);
            }

            // QR historis Pilot di path bawaan MRN dua pasien manifest pertama: informasi saja, bukan konflik.
            TulisFile(PathQrHistorisFisik(MrnUntuk(0)), "QR-HISTORIS:" + MrnUntuk(0));
            TulisFile(PathQrHistorisFisik(MrnUntuk(1)), "QR-HISTORIS:" + MrnUntuk(1));
        }

        private RsmmcBulkApprovedBatchProfile ProfilDariDataBaku()
        {
            long Hitung(string sql)
            {
                using var perintah = _koneksi.CreateCommand();
                perintah.CommandText = sql;
                perintah.Parameters.Add(BuatParameter(perintah, "$b", Teks(BatchKanonik)));
                return Convert.ToInt64(perintah.ExecuteScalar(), CultureInfo.InvariantCulture);
            }

            return RsmmcBulkApprovedBatchProfile.Canonical with
            {
                ExpectedPreviewRows = Hitung("SELECT count(*) FROM migration.rsmmc_patient_target_preview WHERE batch_id = $b"),
                ExpectedFinalizedPlannerRows = Hitung(
                    "SELECT count(*) FROM migration.rsmmc_patient_mrn_plan WHERE batch_id = $b AND plan_status = 'FINALIZED'"),
                ApprovedPatientBaseline = JumlahPasien(),
                ExpectedPilotTargets = PidPilot.Length,
                ExpectedPilotAutoMigrated = 2,
                ExpectedPilotManualMigrated = 1,
                ExpectedPilotPreviewRows = PidPilot.Length,
                ExpectedBackupRows = PidPilot.Length
            };
        }

        private static DbParameter BuatParameter(DbCommand perintah, string nama, object? nilai)
        {
            var p = perintah.CreateParameter();
            p.ParameterName = nama;
            p.Value = nilai ?? DBNull.Value;
            return p;
        }

        private static string Teks(Guid nilai) => nilai.ToString("D");

        private static object? Bit(bool? nilai) => nilai is null ? null : nilai.Value ? 1 : 0;

        private static bool IsSkalar(Type type)
        {
            var dasar = Nullable.GetUnderlyingType(type) ?? type;

            return dasar.IsPrimitive || dasar.IsEnum || dasar == typeof(string) || dasar == typeof(Guid) ||
                dasar == typeof(DateTime) || dasar == typeof(decimal) || dasar == typeof(TimeSpan);
        }
    }
}
