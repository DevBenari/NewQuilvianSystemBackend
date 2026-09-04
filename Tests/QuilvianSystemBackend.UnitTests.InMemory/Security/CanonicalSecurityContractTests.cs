using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Seeders;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.Security;

/// <summary>
/// Kontrak keamanan kanonik Phase A0, dikunci sebagai test.
///
/// <para><b>Aturan 1 — identitas otorisasi.</b> Untuk endpoint yang memiliki
/// <c>[AccessPermission(resource, action)]</c>, pasangan itulah identitas otorisasinya. Seeder
/// mendaftarkan pasangan yang sama, sehingga kunci yang dicari runtime selalu ada di registry.</para>
///
/// <para><b>Aturan 2 — peran <c>[AccessAction]</c>.</b> Pada endpoint yang punya
/// <c>[AccessPermission]</c>, <c>[AccessAction]</c> hanya menyumbang metadata tampilan: nama tampil,
/// deskripsi, urutan, dan <c>AccessType</c> untuk kolom layar Akses Role. Ia <b>tidak boleh</b>
/// mendefinisikan identitas otorisasi yang berbeda, dan memang tidak bisa: argumen pertamanya tidak
/// pernah dipakai sebagai kunci ketika endpoint punya permission.</para>
///
/// <para><b>Aturan 3 — fallback kompatibilitas.</b> Endpoint yang hanya punya <c>[AccessAction]</c>
/// tanpa <c>[AccessPermission]</c> tetap didaftarkan memakai <c>(ControllerName, ActionName)</c>,
/// persis seperti perilaku sebelum Phase A0. Ini <b>perilaku kompatibilitas</b>, bukan pola untuk
/// endpoint baru: endpoint semacam itu tidak ditegakkan matriks Akses Role dan hanya terlindungi
/// <c>[Authorize]</c> atau policy perangkat seperti <c>KioskRead</c>. Mencabut pendaftarannya akan
/// mematikan endpoint saudaranya yang menegakkan kunci yang sama.</para>
/// </summary>
public sealed class CanonicalSecurityContractTests
{
    private static PermissionRegistryDescriptor.RegistrySnapshot Snapshot() =>
        PermissionRegistryDescriptor.BuildFromAssembly(typeof(AccessPermissionService).Assembly);

    /// <summary>
    /// Aturan 1 dan 2: identitas otorisasi selalu berasal dari <c>[AccessPermission]</c>.
    ///
    /// Dibuktikan begini: seluruh kunci yang terdaftar berasal dari salah satu dari dua sumber —
    /// pasangan <c>[AccessPermission]</c>, atau fallback kompatibilitas. Tidak ada kunci ketiga
    /// yang lahir dari argumen pertama <c>[AccessAction]</c> pada endpoint yang punya permission.
    /// </summary>
    [Fact]
    public void AuthorizationIdentityAlwaysComesFromAccessPermission()
    {
        var snapshot = Snapshot();
        var fallbackKeys = snapshot.UnenforcedActions
            .Select(x => PermissionRegistryDescriptor.RegistrySnapshot.Key(x.ResourceName, x.ActionName))
            .ToHashSet(StringComparer.Ordinal);

        // Setiap kunci terdaftar yang BUKAN hasil fallback wajib benar-benar dipakai sebuah
        // [AccessPermission]. Bila tidak, berarti ada sumber identitas kedua yang menyelinap.
        var permissionKeys = snapshot.Actions
            .Select(x => PermissionRegistryDescriptor.RegistrySnapshot.Key(x.ResourceName, x.ActionName))
            .Where(x => !fallbackKeys.Contains(x))
            .ToHashSet(StringComparer.Ordinal);

        Assert.All(permissionKeys, key => Assert.Contains(key, snapshot.DeclaredKeys));
        Assert.Equal(snapshot.DeclaredKeys.Count, snapshot.Actions.Count);
    }

    /// <summary>
    /// Daftar endpoint warisan yang boleh memakai fallback kompatibilitas, disetujui pada
    /// penutupan Phase A0.
    ///
    /// Seluruhnya hanya memiliki <c>[AccessAction]</c> tanpa <c>[AccessPermission]</c>, dan
    /// terlindungi mekanisme lain: policy perangkat <c>KioskRead</c> dan
    /// <c>QueueDisplayRuntimeRead</c>, atau <c>[Authorize]</c> ditambah pembatasan data milik
    /// sendiri pada Self Service.
    ///
    /// Daftar ini <b>tertutup</b>. Endpoint terproteksi yang baru wajib memakai
    /// <c>[AccessPermission]</c>.
    /// </summary>
    private static readonly HashSet<string> ApprovedLegacyFallbackEndpoints = new(StringComparer.Ordinal)
    {
        // Master data kiosk pendaftaran mandiri — policy KioskRead
        "Clinic.GetClinicOptionsForKiosk",
        "Clinic.GetClinicsForKiosk",
        "Clinic.GetFilterMetadataForKiosk",
        "CompanyGuarantor.GetCompanyGuarantorOptionsForKiosk",
        "CompanyGuarantor.GetCompanyGuarantorsForKiosk",
        "CompanyGuarantor.GetFilterMetadataForKiosk",
        "DoctorSchedule.GetAvailableClinicsForKiosk",
        "DoctorSchedule.GetDoctorScheduleOptionsForKiosk",
        "DoctorSchedule.GetDoctorSchedulesForKiosk",
        "DoctorSchedule.GetFilterMetadataForKiosk",
        "InsuranceProvider.GetFilterMetadataForKiosk",
        "InsuranceProvider.GetInsuranceProviderOptionsForKiosk",
        "InsuranceProvider.GetInsuranceProvidersForKiosk",
        "KioskScanSession.CreateFromScanResultForKiosk",
        "KioskScanSession.GetFilterMetadataForKiosk",
        "KioskScanSession.GetSessionOptionsForKiosk",
        "PatientEncounter.CreateEncounterForKiosk",
        "PatientEncounter.GetEncounterOptionsForKiosk",
        "PatientEncounter.GetFilterMetadataForKiosk",
        "Region.GetCitiesForKiosk",
        "Region.GetCityOptionsForKiosk",
        "Region.GetCountriesForKiosk",
        "Region.GetCountryOptionsForKiosk",
        "Region.GetDistrictOptionsForKiosk",
        "Region.GetDistrictsForKiosk",
        "Region.GetFilterMetadataForKiosk",
        "Region.GetPostalCodeOptionsForKiosk",
        "Region.GetPostalCodesForKiosk",
        "Region.GetProvinceOptionsForKiosk",
        "Region.GetProvincesForKiosk",

        // Master data pasien yang dipakai alur kiosk — policy KioskRead
        "Patient.CreatePatient",
        "Patient.GetFilterMetadata",
        "Patient.GetPatientById",
        "Patient.GetPatientOptions",
        "PatientCompanyGuarantor.CreatePatientCompanyGuarantor",
        "PatientCompanyGuarantor.GetFilterMetadata",
        "PatientCompanyGuarantor.GetPatientCompanyGuarantorOptions",
        "PatientCompanyGuarantor.GetPatientCompanyGuarantors",
        "PatientEmergencyContact.CreatePatientEmergencyContact",
        "PatientEmergencyContact.GetFilterMetadata",
        "PatientEmergencyContact.GetPatientEmergencyContactOptions",
        "PatientEmergencyContact.GetPatientEmergencyContacts",
        "PatientIdentityDocument.CreatePatientIdentityDocument",
        "PatientIdentityDocument.GetFilterMetadata",
        "PatientIdentityDocument.GetPatientIdentityDocumentOptions",
        "PatientIdentityDocument.GetPatientIdentityDocuments",
        "PatientInsurance.CreatePatientInsurance",
        "PatientInsurance.GetFilterMetadata",
        "PatientInsurance.GetPatientInsuranceOptions",
        "PatientInsurance.GetPatientInsurances",
        "PatientMembership.CreatePatientMembership",
        "PatientMembership.GetFilterMetadata",
        "PatientMembership.GetPatientMembershipOptions",
        "PatientMembership.GetPatientMemberships",
        "PatientRelationship.CreatePatientRelationship",

        // Layar antrean — policy QueueDisplayRuntimeRead
        "QueueDisplayRuntime.GetCalled",
        "QueueDisplayRuntime.GetCurrent",
        "QueueDisplayRuntime.GetItems",
        "QueueDisplayRuntime.GetSummary",

        // Self Service presensi dan konteks HR — [Authorize] + cakupan data milik sendiri
        "HumanResourceContext.GetCurrent",
        "MyAttendance.CheckIn",
        "MyAttendance.CheckOut",
        "MyAttendance.GetCaptureStatus",
        "MyAttendance.GetDetail",
        "MyAttendance.GetHistory",
        "MyAttendance.GetMetadata",
        "MyAttendance.GetSummary",

        // Audio panggilan antrean — hanya [Authorize]; klasifikasinya masih terbuka
        // (AllowAnonymous versus policy) dan dicatat sebagai keputusan owner yang tersisa.
        "QueueVoice.DownloadAudio",
        "QueueVoice.GetAudio",
    };

    /// <summary>
    /// Aturan 3: fallback kompatibilitas hanya boleh dipakai endpoint warisan yang sudah disetujui.
    ///
    /// Invarian ini membandingkan <b>himpunan</b>, bukan jumlah. Assertion berbasis jumlah masih
    /// meloloskan pertukaran diam-diam: satu endpoint warisan diperbaiki sementara satu endpoint
    /// baru masuk ke fallback, jumlahnya tetap sama. Dengan perbandingan himpunan, keduanya
    /// tertangkap — dan endpoint warisan yang diperbaiki menuntut allowlist diperbarui secara
    /// sadar, bukan diam-diam.
    /// </summary>
    [Fact]
    public void CompatibilityFallbackMatchesApprovedLegacySetExactly()
    {
        var snapshot = Snapshot();

        var actual = snapshot.UnenforcedActions
            .Select(x => $"{x.DeclaringController}.{x.MethodName}")
            .ToHashSet(StringComparer.Ordinal);

        var newlyUsingFallback = actual.Except(ApprovedLegacyFallbackEndpoints, StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal).ToList();

        var noLongerUsingFallback = ApprovedLegacyFallbackEndpoints.Except(actual, StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal).ToList();

        Assert.True(
            newlyUsingFallback.Count == 0,
            "Endpoint berikut memakai fallback kompatibilitas tanpa persetujuan. Endpoint terproteksi " +
            "yang baru wajib memakai [AccessPermission], bukan hanya [AccessAction]:" +
            Environment.NewLine + string.Join(Environment.NewLine, newlyUsingFallback));

        Assert.True(
            noLongerUsingFallback.Count == 0,
            "Endpoint berikut tidak lagi memakai fallback kompatibilitas. Ini kabar baik, tetapi " +
            "allowlist harus diperbarui secara sadar supaya daftarnya tetap menggambarkan keadaan " +
            "sebenarnya:" + Environment.NewLine + string.Join(Environment.NewLine, noLongerUsingFallback));

        // Seluruhnya terdaftar memakai nama controller yang mendeklarasikannya.
        Assert.All(snapshot.UnenforcedActions,
            gap => Assert.Equal(gap.DeclaringController, gap.ResourceName));
    }

    /// <summary>
    /// Endpoint terproteksi tidak boleh kehilangan metadata Akses Role. Bila kuncinya tidak
    /// terdaftar sama sekali, kemampuannya tidak dapat diberikan admin dan endpoint menolak semua
    /// pengguna non-SuperAdmin — persis 89 endpoint yang ditemukan audit.
    /// </summary>
    [Fact]
    public void NoProtectedEndpointIsLeftUnregisterable()
    {
        var result = PermissionRegistryValidator.Validate(Snapshot());
        Assert.Empty(result.UnregisterableEndpoints);
    }

    /// <summary>
    /// Fallback kompatibilitas tidak boleh menghasilkan identitas yang bentrok dengan kunci
    /// permission sungguhan pada modul yang berbeda.
    /// </summary>
    [Fact]
    public void CompatibilityFallbackDoesNotCreateAmbiguousIdentity()
    {
        var result = PermissionRegistryValidator.Validate(Snapshot());
        Assert.Empty(result.DuplicateResourceIdentities);
    }

    /// <summary>
    /// Rekonsiliasi registry tetap tidak membuat <c>SysAccessPolicy</c>, termasuk untuk kunci yang
    /// lahir dari fallback kompatibilitas.
    /// </summary>
    [Fact]
    public async Task CompatibilityFallbackDoesNotGrantAnything()
    {
        await using var db = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"contract-{Guid.NewGuid():N}")
                .Options);

        var snapshot = Snapshot();
        await AccessMenuSeeder.ReconcileAsync(db, snapshot);

        Assert.Equal(0, await db.SysAccessPolicies.CountAsync());

        // Contoh nyata: MyAttendance hanya punya [AccessAction], dilindungi [Authorize] saja.
        var registered = await db.SysActionAccesses
            .AnyAsync(x => x.ControllerAccess!.ControllerName == "MyAttendance");

        Assert.True(registered, "Kunci fallback kompatibilitas harus tetap terdaftar.");
        Assert.Equal(0, await db.SysAccessPolicies.CountAsync());
    }
}
