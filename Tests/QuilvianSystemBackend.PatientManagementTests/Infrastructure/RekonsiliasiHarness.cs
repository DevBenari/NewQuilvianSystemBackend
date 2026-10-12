using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.PatientManagement.Infrastructure
{
    public sealed record PasienPilot(
        string LegacyPid,
        Guid Id,
        string PatientCode,
        string MrnLama,
        string MrnKanonik);

    /// <summary>
    /// Fungsi format QR milik <c>PatientController</c>, dipanggil lewat refleksi supaya uji
    /// memakai format bisnis yang sama persis dengan pembuatan pasien, bukan salinannya.
    /// </summary>
    public static class FormatQrPasien
    {
        public static string Sanitize(string value) => (string)Panggil("SanitizePathSegment", value)!;

        public static string Payload(string mrn) => (string)Panggil("BuildPatientQrPayload", mrn)!;

        public static string Combine(string[] segments) => (string)Panggil("CombineUrlPath", new object[] { segments })!;

        public static byte[] RenderAsli(string payload) =>
            (byte[])Panggil("RenderQrCodePngBytes", payload, null, null)!;

        private static object? Panggil(string nama, params object?[] argumen)
        {
            var method = typeof(PatientController).GetMethod(nama, BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException($"Method {nama} tidak ditemukan pada PatientController.");

            return method.Invoke(null, argumen);
        }
    }

    public sealed class LingkunganUji(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "QuilvianSystemBackend.PatientManagementTests";

        public string ContentRootPath { get; set; } = Path.GetTempPath();

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    /// <summary>
    /// Pasien Pilot, tabel migrasi tiruan, folder QR sementara, dan service rekonsiliasi produksi.
    /// </summary>
    /// <remarks>
    /// Folder QR dibuat di direktori sementara sistem operasi dan dihapus pada
    /// <see cref="Dispose"/>. Uji tidak pernah menulis ke <c>Storage/uploads/</c> repository.
    /// </remarks>
    public sealed class RekonsiliasiHarness : IDisposable
    {
        public static readonly Guid BatchUji = Guid.Parse("0b5e7a10-0000-4000-8000-00000000a001");

        public static readonly Guid AktorUji = Guid.Parse("0b5e7a10-0000-4000-8000-00000000c001");

        public const string PublicRequestPath = "/uploads";

        private static readonly DateTime WaktuDibuat = new(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);
        private static readonly Guid PembuatAwal = Guid.Parse("0b5e7a10-0000-4000-8000-00000000c0de");

        private readonly List<PasienPilot> _pilot = [];

        public RekonsiliasiHarness()
        {
            Database = TestDatabase.Create();
            StorageRoot = Path.Combine(Path.GetTempPath(), "quilvian-pat-mig-uji", Guid.NewGuid().ToString("N"));
        }

        public TestDatabase Database { get; }

        public TiruanSumberMigrasiRsmmc Sumber { get; } = new();

        public string StorageRoot { get; }

        public string EnvironmentName { get; set; } = "Staging";

        public RsmmcPilotApprovedBatchProfile? ProfilKhusus { get; set; }

        public int JumlahResolveStorage { get; private set; }

        public List<string> PayloadDirender { get; } = [];

        /// <summary>Dipanggil saat PNG dibentuk — sesudah pemeriksaan awal, sebelum penulisan akhir.</summary>
        public Action<string>? SaatRender { get; set; }

        public bool GagalRender { get; set; }

        public IReadOnlyList<PasienPilot> Pilot => _pilot;

        public static string PathQrPublik(string mrn) => $"{PublicRequestPath}/patient-qrcodes/{mrn}/qrcode.png";

        public string PathQrFisik(string mrn) => Path.Combine(StorageRoot, "patient-qrcodes", mrn, "qrcode.png");

        public PasienPilot TambahPilot(
            string legacyPid,
            string mrnLama,
            string mrnKanonik,
            string? qrCodePath = null,
            bool tulisQrLama = true)
        {
            var nomor = _pilot.Count + 1;
            var pasien = new PasienPilot(legacyPid, Guid.NewGuid(), $"PAT-RSMMC-{nomor:00000}", mrnLama, mrnKanonik);

            using (var konteks = Database.CreateContext())
            {
                konteks.Set<MstPatient>().Add(new MstPatient
                {
                    Id = pasien.Id,
                    PatientCode = pasien.PatientCode,
                    MedicalRecordNumber = mrnLama,
                    FullName = $"Pasien Uji {nomor}",
                    BirthPlace = "Jakarta",
                    BirthDate = new DateTime(1980, 1, nomor % 28 + 1, 0, 0, 0, DateTimeKind.Utc),
                    IdentityNumber = $"31710000000000{nomor:00}",
                    PhoneNumber = $"08120000{nomor:0000}",
                    Address = $"Jalan Uji No. {nomor}",
                    Notes = "Pilot V1",
                    QrCodePath = qrCodePath ?? PathQrPublik(mrnLama),
                    IsNewborn = false,
                    IsDeceased = false,
                    CreateDateTime = WaktuDibuat,
                    CreateBy = PembuatAwal
                });

                konteks.SaveChanges();
            }

            Sumber.Perencana.Add(new BarisPerencana
            {
                BatchId = BatchUji,
                LegacyPid = legacyPid,
                FinalMedicalRecordNumber = mrnKanonik
            });

            Sumber.Crosswalk.Add(new BarisCrosswalk
            {
                LegacyPid = legacyPid,
                QuilvianPatientId = pasien.Id.ToString("D"),
                NormalizedMrn = "77-77-" + mrnLama[6..]
            });

            Sumber.Cadangan.Add(new BarisCadangan
            {
                BatchId = BatchUji,
                LegacyPid = legacyPid,
                PatientId = pasien.Id,
                OldMedicalRecordNumber = mrnLama,
                PlannedMedicalRecordNumber = mrnKanonik,
                OldQrCodePath = qrCodePath ?? PathQrPublik(mrnLama)
            });

            if (tulisQrLama)
            {
                TulisFile(PathQrFisik(mrnLama), "QR-LAMA:" + mrnLama);
            }

            _pilot.Add(pasien);
            return pasien;
        }

        /// <summary>Pasien biasa di luar Pilot yang memegang sebuah MRN.</summary>
        public Guid TambahPasienLain(string mrn)
        {
            var id = Guid.NewGuid();

            using var konteks = Database.CreateContext();

            konteks.Set<MstPatient>().Add(new MstPatient
            {
                Id = id,
                PatientCode = "PAT-LAIN-" + mrn,
                MedicalRecordNumber = mrn,
                FullName = "Pasien Lain",
                CreateDateTime = WaktuDibuat,
                CreateBy = PembuatAwal
            });

            konteks.SaveChanges();
            return id;
        }

        public RsmmcPilotQrArtifactStore BuatStore()
        {
            return new RsmmcPilotQrArtifactStore(
                new RsmmcPilotQrArtifactLayout(PublicRequestPath, "patient-qrcodes", "qrcode.png"),
                () =>
                {
                    JumlahResolveStorage++;
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
        }

        public RsmmcPilotMrnReconciliationService BuatService(params IInterceptor[] interceptors)
        {
            var profil = ProfilKhusus ?? new RsmmcPilotApprovedBatchProfile(
                BatchUji,
                ExpectedFinalizedPlannerRows: Sumber.Perencana.Count(x => x.BatchId == BatchUji && x.PlanStatus == "FINALIZED") +
                    Sumber.BarisPerencanaNonPilotTambahan,
                ExpectedPilotRows: _pilot.Count,
                ExpectedBackupRows: Sumber.Cadangan.Count(x => x.BatchId == BatchUji));

            return new RsmmcPilotMrnReconciliationService(
                Database.CreateContext(interceptors),
                Sumber,
                new LingkunganUji(EnvironmentName),
                new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()),
                profil);
        }

        public Task<RsmmcPilotMrnReconcileResult> JalankanAsync(
            bool? dryRun,
            int? limit = null,
            Guid? batchId = null,
            params IInterceptor[] interceptors)
        {
            var service = BuatService(interceptors);

            return service.ReconcileAsync(
                new RsmmcPilotMrnReconcileRequest
                {
                    BatchId = batchId ?? BatchUji,
                    DryRun = dryRun,
                    Limit = limit
                },
                AktorUji,
                BuatStore(),
                "uji-trace",
                CancellationToken.None);
        }

        public MstPatient BacaPasien(Guid id)
        {
            using var konteks = Database.CreateContext();

            return konteks.Set<MstPatient>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Single(x => x.Id == id);
        }

        public int JumlahPasien()
        {
            using var konteks = Database.CreateContext();
            return konteks.Set<MstPatient>().IgnoreQueryFilters().Count();
        }

        public List<string> SemuaPatientCode()
        {
            using var konteks = Database.CreateContext();

            return konteks.Set<MstPatient>()
                .IgnoreQueryFilters()
                .Select(x => x.PatientCode)
                .OrderBy(x => x)
                .ToList();
        }

        /// <summary>Seluruh berkas dan folder di bawah folder QR sementara beserta isinya.</summary>
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

            foreach (var file in Directory.EnumerateFiles(StorageRoot, "*", SearchOption.AllDirectories))
            {
                hasil[Path.GetRelativePath(StorageRoot, file)] = File.ReadAllText(file);
            }

            return hasil;
        }

        public static void TulisFile(string path, string isi)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, isi);
        }

        public void Dispose()
        {
            Database.Dispose();

            if (Directory.Exists(StorageRoot))
            {
                Directory.Delete(StorageRoot, recursive: true);
            }
        }
    }
}
