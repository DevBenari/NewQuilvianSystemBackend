using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-08</c> — pendaftaran pasien laboratorium
/// (<c>FR-08.1</c> .. <c>FR-08.5</c>; <c>LAB-DEC-032</c>, <c>LAB-DEC-035</c>).
///
/// Yang dibuktikan di sini:
///   1. <c>AC-44</c> — pendaftaran datang langsung membentuk kunjungan ber-<c>IsWalkIn</c>
///      benar dan bersumber pendaftaran <c>WalkIn</c>;
///   2. <c>AC-45</c> — <b>nol</b> penulisan ke tabel kunjungan maupun tabel pasien dari kode
///      Laboratorium, dibuktikan dua kali: dengan menelusuri seluruh source modul, dan dengan
///      menjalankan jalurnya memakai Registrasi tiruan yang tidak menulis apa pun;
///   3. <c>AC-46</c> — rujukan luar menyimpan penunjuk instansi, penunjuk dokter, dan nomor
///      surat rujukan pada kunjungan yang dibuat Registrasi;
///   4. <c>AC-50</c> / <c>VAL-43</c> — nama instansi tidak dapat diketik bebas, dan penunjuk
///      yang tidak terdaftar ditolak;
///   5. <c>VAL-44</c> nomor surat kosong ditolak, <c>VAL-45</c> idempotensi, dan
///      <c>VAL-41</c> penolakan kewenangan diteruskan apa adanya.
/// </summary>
public class LabPatientRegistrationTests
{
    private static readonly Guid Petugas = Guid.Parse("55555555-5555-5555-5555-555555555555");

    // =====================================================================
    // 1. Bentuk kontrak
    // =====================================================================

    [Theory]
    [InlineData(nameof(LabPatientRegistrationController.SearchPatients), "patient-search", "Read", typeof(HttpGetAttribute))]
    [InlineData(nameof(LabPatientRegistrationController.RegisterWalkIn), "walk-in", "Create", typeof(HttpPostAttribute))]
    [InlineData(nameof(LabPatientRegistrationController.RegisterExternalReferral), "external-referral", "Create", typeof(HttpPostAttribute))]
    public void KetigaEndpoint_CocokDenganKontrakLabApiV1(
        string methodName,
        string template,
        string action,
        Type verbType)
    {
        var method = typeof(LabPatientRegistrationController).GetMethod(methodName);

        Assert.NotNull(method);

        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();

        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);

        Assert.Equal("LabPatientRegistration", arguments[0]);
        Assert.Equal(action, arguments[1]);

        var verb = method.GetCustomAttributes().Single(x => x is HttpMethodAttribute);

        Assert.IsType(verbType, verb);
        Assert.Equal(template, ((HttpMethodAttribute)verb).Template);
    }

    [Fact]
    public void GrupPendaftaran_HanyaPunyaTigaEndpoint()
    {
        var verbs = typeof(LabPatientRegistrationController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(x => x.GetCustomAttributes<HttpMethodAttribute>())
            .ToList();

        Assert.Equal(3, verbs.Count);
        Assert.DoesNotContain(verbs, x => x is HttpPutAttribute);
        Assert.DoesNotContain(verbs, x => x is HttpDeleteAttribute);
    }

    /// <summary>
    /// <c>AC-50</c>. Instansi dan dokter perujuk masuk sebagai <b>penunjuk</b>, dan tidak ada
    /// satu pun ruas teks yang dapat menampung namanya. Inilah penegakan yang sebenarnya:
    /// bukan penjaga yang menolak teks bebas, melainkan bentuk permintaan yang tidak
    /// menyediakan tempat untuk menuliskannya.
    /// </summary>
    [Fact]
    public void AC50_PermintaanRujukan_TidakPunyaRuasNamaPerujukSamaSekali()
    {
        var ruasTeks = typeof(RegisterLabExternalReferralRequest)
            .GetProperties()
            .Where(x => x.PropertyType == typeof(string))
            .Select(x => x.Name)
            .ToList();

        Assert.DoesNotContain(ruasTeks, nama =>
            nama.Contains("InstitutionName", StringComparison.OrdinalIgnoreCase) ||
            nama.Contains("DoctorName", StringComparison.OrdinalIgnoreCase) ||
            nama.Contains("ReferralInstitution", StringComparison.OrdinalIgnoreCase) ||
            nama.Contains("ReferralDoctor", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(
            typeof(Guid?),
            typeof(RegisterLabExternalReferralRequest)
                .GetProperty(nameof(RegisterLabExternalReferralRequest.ReferralInstitutionId))!
                .PropertyType);

        Assert.Equal(
            typeof(Guid?),
            typeof(RegisterLabExternalReferralRequest)
                .GetProperty(nameof(RegisterLabExternalReferralRequest.ReferralDoctorId))!
                .PropertyType);
    }

    // =====================================================================
    // 2. AC-45 — batas kewenangan
    // =====================================================================

    /// <summary>
    /// <c>AC-45</c>, telusur source. Seluruh berkas modul Laboratorium diperiksa, dan tidak
    /// boleh ada satu pun yang membentuk atau mengubah kunjungan maupun data induk pasien.
    ///
    /// Ini penjaga yang paling penting pada task ini: batasnya menggoda untuk ditembus, dan
    /// pelanggarannya berupa satu baris yang tampak wajar.
    /// </summary>
    [Fact]
    public void AC45_TidakSatuPunBerkasLaboratorium_MenulisKeKunjunganAtauDataIndukPasien()
    {
        var akarModul = Path.Combine(
            CariAkarRepository(),
            "Areas", "HealthServices", "LaboratoryManagement");

        Assert.True(Directory.Exists(akarModul), $"Folder modul tidak ditemukan: {akarModul}");

        var berkas = Directory
            .GetFiles(akarModul, "*.cs", SearchOption.AllDirectories)
            .ToList();

        // Tanpa ini, uji lulus hanya karena tidak menemukan berkas sama sekali.
        Assert.NotEmpty(berkas);

        string[] polaTerlarang =
        {
            "new TrxPatientEncounter",
            "new RegPatientEncounterGuarantor",
            "new MstPatient",
            "TrxPatientEncounters.Add",
            "TrxPatientEncounters.Update",
            "TrxPatientEncounters.Remove",
            "MstPatients.Add",
            "MstPatients.Update",
            "MstPatients.Remove",
            "Set<TrxPatientEncounter>().Add",
            "Set<TrxPatientEncounter>().Update",
            "Set<TrxPatientEncounter>().Remove",
            "Set<MstPatient>().Add",
            "Set<MstPatient>().Update",
            "Set<MstPatient>().Remove"
        };

        var temuan = new List<string>();

        foreach (var path in berkas)
        {
            var baris = File.ReadAllLines(path);

            for (var i = 0; i < baris.Length; i++)
            {
                foreach (var pola in polaTerlarang)
                {
                    if (baris[i].Contains(pola, StringComparison.Ordinal))
                    {
                        temuan.Add($"{Path.GetFileName(path)}:{i + 1} -> {pola}");
                    }
                }
            }
        }

        Assert.Empty(temuan);
    }

    /// <summary>
    /// <c>AC-45</c>, telusur perilaku. Laboratorium dan Registrasi diberi <b>dua penyimpanan
    /// yang terpisah</b>, lalu satu pendaftaran dijalankan.
    ///
    /// Kunjungan hanya boleh muncul pada penyimpanan milik Registrasi. Bila Laboratorium
    /// diam-diam menulis sendiri, kunjungan akan muncul pada penyimpanan miliknya — dan uji
    /// ini gagal. Pemisahan itulah yang membuat buktinya tidak dapat dikelabui.
    /// </summary>
    [Fact]
    public async Task AC45_PenulisanKunjungan_DatangDariRegistrasiBukanDariLaboratorium()
    {
        using var labContext = CreateInMemoryContext();
        using var registrasiContext = CreateInMemoryContext();

        var patientId = Guid.NewGuid();
        var serviceUnitId = Guid.NewGuid();

        await SeedPatientAsync(labContext, id: patientId);
        await SeedPatientAsync(registrasiContext, id: patientId);
        await SeedServiceUnitAsync(registrasiContext, id: serviceUnitId);

        var accessor = CreateAccessor();

        var intake = new RegistrasiDenganKewenangan(
            registrasiContext, accessor, CreateLogger(accessor), berwenang: true);

        var service = new LabPatientRegistrationService(labContext, intake);

        await service.RegisterWalkInAsync(new RegisterLabWalkInRequest
        {
            PatientId = patientId,
            ServiceUnitId = serviceUnitId,
            IdempotencyKey = "kunci-ac45"
        });

        // Registrasi yang membuatnya.
        Assert.Single(registrasiContext.Set<TrxPatientEncounter>());
        Assert.Single(registrasiContext.Set<RegPatientEncounterGuarantor>());

        // Laboratorium tidak menulis apa pun — tidak kunjungan, tidak sumber pembayaran,
        // dan tidak pula data induk pasien.
        Assert.Empty(labContext.Set<TrxPatientEncounter>());
        Assert.Empty(labContext.Set<RegPatientEncounterGuarantor>());
        Assert.Single(labContext.Set<MstPatient>());
    }

    // =====================================================================
    // 3. AC-44 dan AC-46 — kunjungan yang terbentuk
    // =====================================================================

    [Fact]
    public async Task AC44_PendaftaranDatangLangsung_MembentukKunjunganWalkInLewatRegistrasi()
    {
        using var context = CreateInMemoryContext();
        var patient = await SeedPatientAsync(context);
        var serviceUnit = await SeedServiceUnitAsync(context);

        var service = CreateService(context, berwenang: true);

        var hasil = await service.RegisterWalkInAsync(new RegisterLabWalkInRequest
        {
            PatientId = patient.Id,
            ServiceUnitId = serviceUnit.Id,
            IdempotencyKey = "kunci-ac44"
        });

        var encounter = Assert.Single(context.Set<TrxPatientEncounter>());

        Assert.Equal(encounter.Id, hasil.EncounterId);
        Assert.Equal(encounter.EncounterNumber, hasil.EncounterNumber);
        Assert.StartsWith("ENC-RSMMC-", hasil.EncounterNumber);
        Assert.False(hasil.IsReplay);

        Assert.True(encounter.IsWalkIn);
        Assert.Equal(EncounterRegistrationSource.WalkIn, encounter.RegistrationSource);
        Assert.Equal(patient.Id, encounter.PatientId);
        Assert.Equal(serviceUnit.Id, encounter.ServiceUnitId);
        Assert.False(encounter.IsReferral);

        // Identitas pasien ikut terbawa supaya layar berikutnya dapat langsung membuat pesanan.
        Assert.Equal(patient.MedicalRecordNumber, hasil.MedicalRecordNumber);
        Assert.Equal(patient.FullName, hasil.FullName);

        // Setiap kunjungan wajib punya tepat satu sumber pembayaran.
        Assert.Single(context.Set<RegPatientEncounterGuarantor>());
    }

    [Fact]
    public async Task AC46_PendaftaranRujukanLuar_MenyimpanPenunjukPerujukDanNomorSurat()
    {
        using var context = CreateInMemoryContext();
        var patient = await SeedPatientAsync(context);
        var serviceUnit = await SeedServiceUnitAsync(context);
        var (institution, doctor) = await SeedReferralAsync(context);

        var service = CreateService(context, berwenang: true);

        await service.RegisterExternalReferralAsync(new RegisterLabExternalReferralRequest
        {
            PatientId = patient.Id,
            ServiceUnitId = serviceUnit.Id,
            IdempotencyKey = "kunci-ac46",
            ReferralNumber = "RJK/2026/00123",
            ReferralInstitutionId = institution.Id,
            ReferralDoctorId = doctor.Id
        });

        var encounter = Assert.Single(context.Set<TrxPatientEncounter>());

        Assert.True(encounter.IsReferral);
        Assert.Equal("RJK/2026/00123", encounter.ReferralNumber);
        Assert.Equal(institution.Id, encounter.ReferralInstitutionId);
        Assert.Equal(doctor.Id, encounter.ReferralDoctorId);
    }

    // =====================================================================
    // 4. Jalur gagal
    // =====================================================================

    /// <summary><c>VAL-43</c> dan <c>AC-50</c>: penunjuk yang tidak terdaftar ditolak.</summary>
    [Fact]
    public async Task VAL43_InstansiPerujukTidakTerdaftar_Ditolak()
    {
        using var context = CreateInMemoryContext();
        var patient = await SeedPatientAsync(context);
        var serviceUnit = await SeedServiceUnitAsync(context);

        var service = CreateService(context, berwenang: true);

        var exception = await Assert.ThrowsAsync<EncounterIntakeValidationException>(() =>
            service.RegisterExternalReferralAsync(new RegisterLabExternalReferralRequest
            {
                PatientId = patient.Id,
                ServiceUnitId = serviceUnit.Id,
                IdempotencyKey = "kunci-val43",
                ReferralNumber = "RJK/2026/00124",
                ReferralInstitutionId = Guid.NewGuid(),
                ReferralDoctorId = Guid.NewGuid()
            }));

        Assert.Contains("Pilih instansi perujuk dari daftar", exception.Message);
        Assert.Empty(context.Set<TrxPatientEncounter>());
    }

    /// <summary><c>VAL-44</c>: nomor surat rujukan kosong ditolak.</summary>
    [Fact]
    public async Task VAL44_NomorSuratRujukanKosong_Ditolak()
    {
        using var context = CreateInMemoryContext();
        var patient = await SeedPatientAsync(context);
        var serviceUnit = await SeedServiceUnitAsync(context);
        var (institution, doctor) = await SeedReferralAsync(context);

        var service = CreateService(context, berwenang: true);

        var exception = await Assert.ThrowsAsync<LabPatientRegistrationValidationException>(() =>
            service.RegisterExternalReferralAsync(new RegisterLabExternalReferralRequest
            {
                PatientId = patient.Id,
                ServiceUnitId = serviceUnit.Id,
                IdempotencyKey = "kunci-val44",
                ReferralNumber = "   ",
                ReferralInstitutionId = institution.Id,
                ReferralDoctorId = doctor.Id
            }));

        Assert.Contains("Nomor surat rujukan wajib diisi", exception.Message);
        Assert.Empty(context.Set<TrxPatientEncounter>());
    }

    /// <summary>
    /// <c>VAL-41</c>. Hak akses pada layar Laboratorium hanya membuka layarnya; kewenangan
    /// membuat kunjungan tetap milik Registrasi dan penolakannya diteruskan apa adanya.
    /// </summary>
    [Fact]
    public async Task VAL41_TanpaKewenanganRegistrasi_PendaftaranDitolakDanTidakMenyimpanApaPun()
    {
        using var context = CreateInMemoryContext();
        var patient = await SeedPatientAsync(context);
        var serviceUnit = await SeedServiceUnitAsync(context);

        var service = CreateService(context, berwenang: false);

        var exception = await Assert.ThrowsAsync<EncounterIntakeForbiddenException>(() =>
            service.RegisterWalkInAsync(new RegisterLabWalkInRequest
            {
                PatientId = patient.Id,
                ServiceUnitId = serviceUnit.Id,
                IdempotencyKey = "kunci-val41"
            }));

        Assert.Contains("tidak berhak membuat kunjungan baru", exception.Message);
        Assert.Empty(context.Set<TrxPatientEncounter>());
        Assert.Empty(context.Set<RegPatientEncounterGuarantor>());
    }

    /// <summary>
    /// Penolakan Registrasi tidak boleh meninggalkan data setengah jadi. Pasien yang tidak
    /// dikenal ditolak, dan tidak ada satu baris pun — termasuk sumber pembayaran — tersisa.
    /// </summary>
    [Fact]
    public async Task PenolakanRegistrasi_TidakMeninggalkanDataSetengahJadi()
    {
        using var context = CreateInMemoryContext();
        var serviceUnit = await SeedServiceUnitAsync(context);

        var service = CreateService(context, berwenang: true);

        await Assert.ThrowsAsync<EncounterIntakeValidationException>(() =>
            service.RegisterWalkInAsync(new RegisterLabWalkInRequest
            {
                PatientId = Guid.NewGuid(),
                ServiceUnitId = serviceUnit.Id,
                IdempotencyKey = "kunci-tolak"
            }));

        Assert.Empty(context.Set<TrxPatientEncounter>());
        Assert.Empty(context.Set<RegPatientEncounterGuarantor>());
    }

    [Fact]
    public async Task KunciIdempotensiKosong_Ditolak()
    {
        using var context = CreateInMemoryContext();
        var service = CreateService(context, berwenang: true);

        await Assert.ThrowsAsync<LabPatientRegistrationValidationException>(() =>
            service.RegisterWalkInAsync(new RegisterLabWalkInRequest
            {
                PatientId = Guid.NewGuid(),
                ServiceUnitId = Guid.NewGuid(),
                IdempotencyKey = null
            }));
    }

    // =====================================================================
    // 5. VAL-45 — idempotensi
    // =====================================================================

    /// <summary>
    /// <c>VAL-45</c> dan butir DoD idempotensi. Petugas menekan Simpan, jaringan lambat, lalu
    /// ia menekan Simpan lagi. Yang terbentuk tetap <b>satu</b> kunjungan.
    /// </summary>
    [Fact]
    public async Task VAL45_MenekanSimpanDuaKali_MenghasilkanSatuKunjunganYangSama()
    {
        using var context = CreateInMemoryContext();
        var patient = await SeedPatientAsync(context);
        var serviceUnit = await SeedServiceUnitAsync(context);

        var service = CreateService(context, berwenang: true);

        RegisterLabWalkInRequest Permintaan() => new()
        {
            PatientId = patient.Id,
            ServiceUnitId = serviceUnit.Id,
            IdempotencyKey = "kunci-val45"
        };

        var pertama = await service.RegisterWalkInAsync(Permintaan());
        var kedua = await service.RegisterWalkInAsync(Permintaan());

        Assert.Equal(pertama.EncounterId, kedua.EncounterId);
        Assert.Equal(pertama.EncounterNumber, kedua.EncounterNumber);

        Assert.False(pertama.IsReplay);
        Assert.True(kedua.IsReplay);

        Assert.Single(context.Set<TrxPatientEncounter>());
        Assert.Single(context.Set<RegPatientEncounterGuarantor>());
    }

    [Fact]
    public async Task DuaPercobaanBerbeda_MenghasilkanDuaKunjungan()
    {
        using var context = CreateInMemoryContext();
        var patient = await SeedPatientAsync(context);
        var serviceUnit = await SeedServiceUnitAsync(context);

        var service = CreateService(context, berwenang: true);

        // Kunci yang berbeda berarti percobaan pendaftaran yang berbeda — dan itu memang boleh
        // menghasilkan kunjungan kedua. Idempotensi menjaga pengiriman ulang, bukan melarang
        // pasien datang dua kali.
        await service.RegisterWalkInAsync(new RegisterLabWalkInRequest
        {
            PatientId = patient.Id,
            ServiceUnitId = serviceUnit.Id,
            IdempotencyKey = "percobaan-1"
        });

        await service.RegisterWalkInAsync(new RegisterLabWalkInRequest
        {
            PatientId = patient.Id,
            ServiceUnitId = serviceUnit.Id,
            IdempotencyKey = "percobaan-2"
        });

        Assert.Equal(2, context.Set<TrxPatientEncounter>().Count());
    }

    // =====================================================================
    // 6. Pencarian pasien
    // =====================================================================

    [Fact]
    public async Task PencarianPasien_MenemukanLewatNomorRekamMedisDanNama()
    {
        using var context = CreateInMemoryContext();
        var patient = await SeedPatientAsync(context, nama: "Siti Rahayu", noRm: "RM-000771");
        await SeedPatientAsync(context, nama: "Budi Santoso", noRm: "RM-000772");

        var service = CreateService(context, berwenang: true);

        var lewatRm = await service.SearchPatientsAsync(new LabPatientSearchQuery { Search = "RM-000771" });
        var lewatNama = await service.SearchPatientsAsync(new LabPatientSearchQuery { Search = "Siti" });

        Assert.Equal(patient.Id, Assert.Single(lewatRm).PatientId);
        Assert.Equal(patient.Id, Assert.Single(lewatNama).PatientId);
    }

    // =====================================================================
    // 7. Bentuk penyimpanan
    // =====================================================================

    /// <summary>
    /// Idempotensi tidak boleh bergantung pada kode aplikasi semata. Unique index tersaring
    /// inilah yang memutuskan ketika dua permintaan berkunci sama tiba bersamaan.
    /// </summary>
    [Fact]
    public void KunciIdempotensi_PunyaUniqueIndexTersaringPadaTabelKunjungan()
    {
        using var context = CreateRelationalModelContext();

        var entity = context.Model.FindEntityType(typeof(TrxPatientEncounter));

        Assert.NotNull(entity);

        var property = entity!.FindProperty(nameof(TrxPatientEncounter.RegistrationIdempotencyKey));

        Assert.NotNull(property);
        Assert.True(property!.IsNullable);
        Assert.Equal(100, property.GetMaxLength());

        var index = entity.GetIndexes()
            .SingleOrDefault(x =>
                x.Properties.Count == 1 &&
                x.Properties[0].Name == nameof(TrxPatientEncounter.RegistrationIdempotencyKey));

        Assert.NotNull(index);
        Assert.True(index!.IsUnique);
        Assert.Contains("IS NOT NULL", index.GetFilter() ?? string.Empty);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    /// <summary>
    /// Registrasi sungguhan, dengan satu-satunya bagian yang digantikan adalah pemeriksaan
    /// kewenangan — supaya uji tidak perlu menyusun seluruh struktur RBAC.
    /// </summary>
    private sealed class RegistrasiDenganKewenangan(
        ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService,
        bool berwenang)
        : EncounterIntakeService(dbContext, httpContextAccessor, null!, loggerService)
    {
        protected override Task<bool> HasRegistrationAuthorityAsync(CancellationToken cancellationToken) =>
            Task.FromResult(berwenang);
    }

    private static LabPatientRegistrationService CreateService(
        ApplicationDbContext context,
        bool berwenang)
    {
        var accessor = CreateAccessor();

        var intake = new RegistrasiDenganKewenangan(
            context, accessor, CreateLogger(accessor), berwenang);

        return new LabPatientRegistrationService(context, intake);
    }

    private static HttpContextAccessor CreateAccessor()
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, Petugas.ToString()) },
            authenticationType: "LabPatientRegistrationTest");

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static LoggerService CreateLogger(IHttpContextAccessor? accessor = null) =>
        new(NullLogger<LoggerService>.Instance, accessor ?? new HttpContextAccessor());

    private static async Task<MstPatient> SeedPatientAsync(
        ApplicationDbContext context,
        string nama = "Pasien Uji",
        string? noRm = null,
        Guid? id = null)
    {
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var patient = new MstPatient
        {
            Id = id ?? Guid.NewGuid(),
            PatientCode = $"PSN-{suffix}",
            MedicalRecordNumber = noRm ?? $"RM-{suffix}",
            FullName = nama,
            BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true,
            IsDelete = false
        };

        context.Set<MstPatient>().Add(patient);
        await context.SaveChangesAsync();

        return patient;
    }

    private static async Task<MstServiceUnit> SeedServiceUnitAsync(
        ApplicationDbContext context,
        Guid? id = null)
    {
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var serviceUnit = new MstServiceUnit
        {
            Id = id ?? Guid.NewGuid(),
            ServiceUnitCode = $"LAB-{suffix}",
            ServiceUnitName = "Laboratorium",
            IsActive = true,
            IsDelete = false
        };

        context.Set<MstServiceUnit>().Add(serviceUnit);
        await context.SaveChangesAsync();

        return serviceUnit;
    }

    private static async Task<(MstReferralInstitution Institution, MstReferralDoctor Doctor)>
        SeedReferralAsync(ApplicationDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var institution = new MstReferralInstitution
        {
            Id = Guid.NewGuid(),
            InstitutionCode = $"INS-{suffix}",
            InstitutionName = "Klinik Sehat Sentosa",
            IsActive = true,
            IsDelete = false
        };

        var doctor = new MstReferralDoctor
        {
            Id = Guid.NewGuid(),
            ReferralInstitutionId = institution.Id,
            DoctorName = "dr. Andi Wijaya",
            IsActive = true,
            IsDelete = false
        };

        context.Set<MstReferralInstitution>().Add(institution);
        context.Set<MstReferralDoctor>().Add(doctor);
        await context.SaveChangesAsync();

        return (institution, doctor);
    }

    private static string CariAkarRepository()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null &&
               !File.Exists(Path.Combine(directory.FullName, "QuilvianSystemBackend.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        return directory!.FullName;
    }

    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-patient-registration-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationDbContext CreateRelationalModelContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=lab_patient_registration_model_only")
            .Options;

        return new ApplicationDbContext(options);
    }
}
