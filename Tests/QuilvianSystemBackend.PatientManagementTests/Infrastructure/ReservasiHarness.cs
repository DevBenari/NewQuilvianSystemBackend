using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.PatientManagement.Infrastructure
{
    /// <summary>Environment web uji: nama environment dan folder sementara saja.</summary>
    public sealed class LingkunganWebUji(string environmentName, string root) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "QuilvianSystemBackend.PatientManagementTests";

        public string ContentRootPath { get; set; } = root;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public string WebRootPath { get; set; } = root;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }

    /// <summary>Bentuk konfigurasi guard reservasi pada uji (`PAT-DEC-066`, `PAT-DEC-067`).</summary>
    public enum KonfigurasiGuardUji
    {
        BagianTidakAda,
        KunciEnabledTidakAda,
        EnabledTrue,
        EnabledFalse,
        EnabledSalahBentuk
    }

    /// <summary>
    /// Konfigurasi yang selalu melempar galat saat dibaca — mewakili konfigurasi yang tidak terbaca.
    /// </summary>
    public sealed class KonfigurasiTidakTerbaca : IConfiguration
    {
        public string? this[string key]
        {
            get => throw new InvalidOperationException("Simulasi penyedia konfigurasi gagal dibaca.");
            set => throw new InvalidOperationException("Simulasi penyedia konfigurasi gagal dibaca.");
        }

        public IEnumerable<IConfigurationSection> GetChildren() =>
            throw new InvalidOperationException("Simulasi penyedia konfigurasi gagal dibaca.");

        public IChangeToken GetReloadToken() => NullChangeToken.Singleton;

        public IConfigurationSection GetSection(string key) =>
            throw new InvalidOperationException("Simulasi penyedia konfigurasi gagal dibaca.");
    }

    /// <summary>
    /// Basis data SQLite, konfigurasi guard, folder unggahan sementara, serta
    /// <see cref="PatientController"/> dan <see cref="PatientIdentifierAllocationService"/> asli.
    /// </summary>
    /// <remarks>
    /// Folder unggahan dibuat di direktori sementara sistem operasi dan dihapus pada
    /// <see cref="Dispose"/>. Uji tidak pernah menulis ke <c>Storage/uploads/</c> repository dan tidak
    /// pernah menyentuh basis data mana pun yang tercatat di appsettings.
    /// </remarks>
    public sealed class ReservasiHarness : IDisposable
    {
        public static readonly Guid AktorUji = Guid.Parse("0b5e7a10-0000-4000-8000-00000000d003");

        private static readonly DateTime WaktuDibuat = new(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);

        public ReservasiHarness(
            string environmentName,
            KonfigurasiGuardUji konfigurasi,
            string nilaiSalahBentuk = "yes")
        {
            Database = TestDatabase.Create();
            StorageRoot = Path.Combine(Path.GetTempPath(), "quilvian-pat-mig3-uji", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(StorageRoot);
            Environment = new LingkunganWebUji(environmentName, StorageRoot);
            Configuration = BuatKonfigurasi(konfigurasi, nilaiSalahBentuk, StorageRoot);
        }

        public TestDatabase Database { get; }

        public string StorageRoot { get; }

        public LingkunganWebUji Environment { get; }

        public IConfiguration Configuration { get; set; }

        public static IConfiguration BuatKonfigurasi(
            KonfigurasiGuardUji konfigurasi,
            string nilaiSalahBentuk = "yes",
            string? storageRoot = null)
        {
            var nilai = new Dictionary<string, string?>();

            if (storageRoot is not null)
            {
                nilai["FileStorage:UploadRootPath"] = storageRoot;
                nilai["FileStorage:PublicRequestPath"] = "/uploads";
            }

            const string bagian = RsmmcMigrationReservationGuardOptions.SectionName;

            switch (konfigurasi)
            {
                case KonfigurasiGuardUji.KunciEnabledTidakAda:
                    nilai[$"{bagian}:Catatan"] = "kunci Enabled sengaja tidak diisi";
                    break;
                case KonfigurasiGuardUji.EnabledTrue:
                    nilai[$"{bagian}:Enabled"] = "true";
                    break;
                case KonfigurasiGuardUji.EnabledFalse:
                    nilai[$"{bagian}:Enabled"] = "false";
                    break;
                case KonfigurasiGuardUji.EnabledSalahBentuk:
                    nilai[$"{bagian}:Enabled"] = nilaiSalahBentuk;
                    break;
            }

            return new ConfigurationBuilder().AddInMemoryCollection(nilai).Build();
        }

        public PatientIdentifierAllocationService BuatService(ApplicationDbContext konteks) =>
            new(konteks, Environment, Configuration);

        public PatientController BuatController(
            ApplicationDbContext konteks,
            NewbornPatientRegistrationService? layananBayi = null)
        {
            var controller = new PatientController(
                konteks,
                new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()),
                Environment,
                Configuration,
                new MedicalRecordAccessAuditService(konteks),
                BuatService(konteks),
                layananBayi);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    TraceIdentifier = "uji-be-pat-mig-003",
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, AktorUji.ToString())],
                        "Uji"))
                }
            };

            return controller;
        }

        public void TambahPasien(string patientCode, string medicalRecordNumber, bool terhapus = false)
        {
            using var konteks = Database.CreateContext();

            konteks.Set<MstPatient>().Add(new MstPatient
            {
                Id = Guid.NewGuid(),
                PatientCode = patientCode,
                MedicalRecordNumber = medicalRecordNumber,
                FullName = "Pasien Uji Reservasi",
                IsDelete = terhapus,
                DeleteDateTime = terhapus ? WaktuDibuat : null,
                CreateDateTime = WaktuDibuat,
                CreateBy = AktorUji
            });

            konteks.SaveChanges();
        }

        public int JumlahPasien()
        {
            using var konteks = Database.CreateContext();

            return konteks.Set<MstPatient>().IgnoreQueryFilters().Count();
        }

        public int JumlahBerkasUnggahan() =>
            Directory.Exists(StorageRoot)
                ? Directory.EnumerateFiles(StorageRoot, "*", SearchOption.AllDirectories).Count()
                : 0;

        public void Dispose()
        {
            Database.Dispose();

            try
            {
                if (Directory.Exists(StorageRoot)) Directory.Delete(StorageRoot, recursive: true);
            }
            catch (IOException)
            {
                // Folder sementara milik uji; sisa penguncian berkas tidak boleh menggagalkan uji.
            }
        }
    }

    /// <summary>
    /// Salinan apa adanya pembangkit nomor lama <c>PatientController</c> pada commit
    /// <c>a52b3c98</c> (<c>GeneratePatientCodeAsync</c>, <c>GenerateRunningCodeAsync</c>,
    /// <c>GenerateMedicalRecordNumberAsync</c>), dipakai sebagai pembanding bahwa perilaku di luar
    /// <c>Staging</c> persis tidak berubah (`PAT-MIG3-AC-08`).
    /// </summary>
    public static class PembangkitLamaAcuan
    {
        public static (string PatientCode, string MedicalRecordNumber) Hitung(IEnumerable<(string Code, string Mrn)> pasien)
        {
            const string prefix = "PAT-RSMMC-";
            var daftar = pasien.ToList();

            var usedCodes = daftar
                .Select(x => x.Code)
                .Where(x => x.StartsWith(prefix))
                .Select(x => x.Replace(prefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextCode = 1;
            while (usedCodes.Contains(nextCode)) nextCode++;

            var usedMrn = daftar
                .Select(x => x.Mrn)
                .Where(x => x != null && x != string.Empty)
                .Select(PatientQrPayloadBuilder.NormalizeToRawDigits)
                .Where(x => x != null)
                .Select(x => int.Parse(x!))
                .Where(x => x > 0)
                .ToHashSet();

            var nextMrn = 1;
            while (usedMrn.Contains(nextMrn)) nextMrn++;

            return (
                prefix + nextCode.ToString().PadLeft(5, '0'),
                PatientQrPayloadBuilder.FormatMedicalRecordNumber(nextMrn.ToString().PadLeft(8, '0')));
        }
    }
}
