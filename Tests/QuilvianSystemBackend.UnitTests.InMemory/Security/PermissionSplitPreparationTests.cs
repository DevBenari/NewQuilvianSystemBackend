using System.Reflection;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Seeders;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.Security;

/// <summary>
/// Invarian pemecahan identitas technical permission — <c>BE-SEC-003A</c>.
///
/// <para>Sebelum pemecahan, satu izin membuka banyak endpoint yang maknanya berbeda. Contoh
/// terparah pada pilot Dokter Rawat Jalan: <c>PatientProcedure.Update</c> membuka lima endpoint
/// sekaligus, termasuk <b>menyetujui</b> dan <b>melaksanakan</b> tindakan pasien. Akibatnya admin
/// tidak dapat memberikan "boleh menghapus pilihan tindakan dari draft" tanpa sekaligus memberikan
/// "boleh menyetujui tindakan".</para>
///
/// <para>Fase A ini <b>hanya menyiapkan identitas dan pemetaan endpoint</b>. Ia sengaja
/// <b>tidak</b> memindahkan satu pun hak: perluasan <c>SysAccessPolicy</c> adalah fase terpisah
/// yang berjalan di luar seeder.</para>
/// </summary>
public sealed class PermissionSplitPreparationTests
{
    private static readonly Assembly BackendAssembly = typeof(AccessPermissionService).Assembly;

    /// <summary>
    /// 22 identitas baru hasil pemecahan pilot Dokter Rawat Jalan.
    /// Dikunci sebagai himpunan persis supaya penambahan diam-diam menggagalkan test.
    /// </summary>
    private static readonly (string Resource, string Action)[] NewSplitIdentities =
    {
        ("PatientProcedure", "Select"),
        ("PatientProcedure", "Edit"),
        ("PatientProcedure", "Approve"),
        ("PatientProcedure", "Execute"),
        ("PatientProcedure", "RemoveDraft"),
        ("PatientProcedure", "Cancel"),

        ("DoctorConsultation", "Complete"),
        ("DoctorConsultation", "Cancel"),

        ("DoctorQueue", "Call"),
        ("DoctorQueue", "StartConsultation"),
        ("DoctorQueue", "FinishConsultation"),
        ("DoctorQueue", "Skip"),
        ("DoctorQueue", "NoShow"),
        ("DoctorQueue", "Requeue"),

        ("PatientVitalSign", "Verify"),
        ("PatientVitalSign", "NotifyDoctor"),
        ("PatientVitalSign", "Cancel"),

        ("PatientAssessment", "Complete"),
        ("PatientAssessment", "Cancel"),

        ("PatientDiagnosis", "SetPrimary"),
        ("PatientDiagnosis", "Resolve"),
        ("PatientDiagnosis", "Cancel"),
    };

    /// <summary>
    /// Identitas lama yang <b>bertahan</b> karena masih dipakai endpoint lain.
    ///
    /// <para>Lima identitas ini adalah alasan pemecahan pilot jauh lebih aman daripada terlihat:
    /// yang benar-benar berhenti dideklarasikan hanya dua, bukan tujuh.</para>
    /// </summary>
    private static readonly (string Resource, string Action)[] SurvivingLegacyIdentities =
    {
        ("DoctorConsultation", "Update"),
        ("PatientVitalSign", "Update"),
        ("PatientAssessment", "Update"),
        ("PatientDiagnosis", "Update"),
        ("PatientProcedure", "Create"),
    };

    /// <summary>
    /// Identitas lama yang berhenti dideklarasikan source, sehingga akan ditutup seeder.
    /// Hak yang menunjuknya dipulihkan pada fase perluasan, bukan di fase ini.
    /// </summary>
    private static readonly (string Resource, string Action)[] RetiredLegacyIdentities =
    {
        ("PatientProcedure", "Update"),
        ("DoctorQueue", "Update"),
    };

    /// <summary>Pemetaan endpoint ke identitas barunya, per nama method controller.</summary>
    private static readonly (string Controller, string Method, string Resource, string Action)[] EndpointIdentityMap =
    {
        ("PatientProcedureController", "SelectProcedure", "PatientProcedure", "Select"),
        ("PatientProcedureController", "CreateProcedure", "PatientProcedure", "Create"),
        ("PatientProcedureController", "UpdateProcedure", "PatientProcedure", "Edit"),
        ("PatientProcedureController", "ApproveProcedure", "PatientProcedure", "Approve"),
        ("PatientProcedureController", "ExecuteProcedure", "PatientProcedure", "Execute"),
        ("PatientProcedureController", "RemoveDraftProcedure", "PatientProcedure", "RemoveDraft"),
        ("PatientProcedureController", "CancelProcedure", "PatientProcedure", "Cancel"),

        ("DoctorConsultationController", "UpdateConsultation", "DoctorConsultation", "Update"),
        ("DoctorConsultationController", "UpdateSoap", "DoctorConsultation", "Update"),
        ("DoctorConsultationController", "CompleteConsultation", "DoctorConsultation", "Complete"),
        ("DoctorConsultationController", "CancelConsultation", "DoctorConsultation", "Cancel"),

        ("DoctorQueueController", "Call", "DoctorQueue", "Call"),
        ("DoctorQueueController", "StartConsultation", "DoctorQueue", "StartConsultation"),
        ("DoctorQueueController", "FinishConsultation", "DoctorQueue", "FinishConsultation"),
        ("DoctorQueueController", "Skip", "DoctorQueue", "Skip"),
        ("DoctorQueueController", "NoShow", "DoctorQueue", "NoShow"),
        ("DoctorQueueController", "Requeue", "DoctorQueue", "Requeue"),

        ("PatientVitalSignController", "UpdateVitalSign", "PatientVitalSign", "Update"),
        ("PatientVitalSignController", "VerifyVitalSign", "PatientVitalSign", "Verify"),
        ("PatientVitalSignController", "NotifyDoctor", "PatientVitalSign", "NotifyDoctor"),
        ("PatientVitalSignController", "CancelVitalSign", "PatientVitalSign", "Cancel"),

        ("PatientAssessmentController", "UpdateAssessment", "PatientAssessment", "Update"),
        ("PatientAssessmentController", "CompleteAssessment", "PatientAssessment", "Complete"),
        ("PatientAssessmentController", "CancelAssessment", "PatientAssessment", "Cancel"),

        ("PatientDiagnosisController", "UpdateDiagnosis", "PatientDiagnosis", "Update"),
        ("PatientDiagnosisController", "SetPrimary", "PatientDiagnosis", "SetPrimary"),
        ("PatientDiagnosisController", "ResolveDiagnosis", "PatientDiagnosis", "Resolve"),
        ("PatientDiagnosisController", "CancelDiagnosis", "PatientDiagnosis", "Cancel"),
    };

    private static PermissionRegistryDescriptor.RegistrySnapshot Snapshot() =>
        PermissionRegistryDescriptor.BuildFromAssembly(BackendAssembly);

    private static ApplicationDbContext NewContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"split-{Guid.NewGuid()}")
            .Options);

    private static MethodInfo FindMethod(string controllerName, string methodName)
    {
        var type = BackendAssembly.GetTypes().Single(x => x.Name == controllerName);
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Single(x => x.Name == methodName);
    }

    /// <summary>
    /// Tuntutan 1: seluruh technical permission baru terdaftar dan dapat dicentang admin.
    /// </summary>
    [Fact]
    public void EveryNewSplitIdentityIsRegistered()
    {
        var snapshot = Snapshot();

        var missing = NewSplitIdentities
            .Where(x => !snapshot.DeclaredKeys.Contains(
                PermissionRegistryDescriptor.RegistrySnapshot.Key(x.Resource, x.Action)))
            .Select(x => $"{x.Resource}.{x.Action}")
            .ToList();

        Assert.True(
            missing.Count == 0,
            "Identitas hasil pemecahan berikut tidak terdaftar di registry, sehingga tidak akan " +
            "muncul di layar Akses Role: " + string.Join(", ", missing));

        Assert.Equal(22, NewSplitIdentities.Length);
    }

    /// <summary>
    /// Tuntutan 2: tidak ada identitas kanonik ganda.
    ///
    /// <para>Setiap identitas <b>baru</b> hasil pemecahan wajib menjaga tepat satu endpoint. Bila
    /// satu identitas baru menjaga lebih dari satu endpoint, pemecahannya belum benar-benar
    /// memisahkan kemampuan.</para>
    /// </summary>
    [Fact]
    public void EveryNewSplitIdentityGuardsExactlyOneEndpoint()
    {
        var usage = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var entry in EndpointIdentityMap)
        {
            var key = $"{entry.Resource}.{entry.Action}";
            if (!usage.TryGetValue(key, out var methods))
            {
                methods = new List<string>();
                usage[key] = methods;
            }

            methods.Add($"{entry.Controller}.{entry.Method}");
        }

        var offenders = NewSplitIdentities
            .Select(x => $"{x.Resource}.{x.Action}")
            .Where(key => usage.TryGetValue(key, out var methods) && methods.Count != 1)
            .Select(key => $"{key} dijaga {usage[key].Count} endpoint: {string.Join(", ", usage[key])}")
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "Identitas baru harus menjaga tepat satu endpoint:" + Environment.NewLine +
            string.Join(Environment.NewLine, offenders));
    }

    /// <summary>
    /// Tuntutan 2 lanjutan: satu nama resource tidak boleh terdaftar pada lebih dari satu modul.
    /// Aturan kanonik <c>BE-SEC-001</c> nomor 5.
    /// </summary>
    [Fact]
    public void NoResourceIsDeclaredInMoreThanOneModule()
    {
        var result = PermissionRegistryValidator.Validate(Snapshot());

        Assert.True(
            result.DuplicateResourceIdentities.Count == 0,
            "Nama resource terdaftar di lebih dari satu modul: " +
            string.Join(", ", result.DuplicateResourceIdentities));
    }

    /// <summary>
    /// Tuntutan 3: endpoint benar-benar memakai identitas barunya.
    ///
    /// <para>Diperiksa langsung dari atribut pada method, bukan dari snapshot, supaya kesalahan
    /// pemasangan atribut tidak tertutupi oleh pendaftaran endpoint saudaranya.</para>
    /// </summary>
    [Fact]
    public void SplitEndpointsUseTheirNewIdentity()
    {
        var failures = new List<string>();

        foreach (var entry in EndpointIdentityMap)
        {
            var method = FindMethod(entry.Controller, entry.Method);
            var permission = method.GetCustomAttribute<AccessPermissionAttribute>();

            if (permission?.Arguments is not { Length: 2 } arguments)
            {
                failures.Add($"{entry.Controller}.{entry.Method} tidak punya [AccessPermission] yang sah.");
                continue;
            }

            var resource = arguments[0] as string;
            var action = arguments[1] as string;

            if (!string.Equals(resource, entry.Resource, StringComparison.Ordinal) ||
                !string.Equals(action, entry.Action, StringComparison.Ordinal))
            {
                failures.Add(
                    $"{entry.Controller}.{entry.Method} memakai {resource}.{action}, " +
                    $"seharusnya {entry.Resource}.{entry.Action}.");
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    /// <summary>
    /// Argumen ke-2 <c>[AccessPermission]</c> wajib sama persis dengan argumen ke-1
    /// <c>[AccessAction]</c> pada method yang sama.
    ///
    /// <para>Bila menyimpang, kemampuannya tetap muncul di layar Akses Role dengan nama lain,
    /// dan hasilnya 403 permanen yang tidak dapat diperbaiki admin — persis 89 endpoint rusak yang
    /// diperbaiki <c>BE-SEC-001</c>.</para>
    /// </summary>
    [Fact]
    public void AccessPermissionActionMatchesAccessActionName()
    {
        var failures = new List<string>();

        foreach (var entry in EndpointIdentityMap)
        {
            var method = FindMethod(entry.Controller, entry.Method);
            var permission = method.GetCustomAttribute<AccessPermissionAttribute>();
            var action = method.GetCustomAttribute<AccessActionAttribute>();

            var permissionAction = permission?.Arguments is { Length: 2 } arguments
                ? arguments[1] as string
                : null;

            if (action is null)
            {
                failures.Add($"{entry.Controller}.{entry.Method} tidak punya [AccessAction].");
                continue;
            }

            if (!string.Equals(action.ActionName, permissionAction, StringComparison.Ordinal))
            {
                failures.Add(
                    $"{entry.Controller}.{entry.Method}: [AccessAction] \"{action.ActionName}\" " +
                    $"tidak sama dengan [AccessPermission] \"{permissionAction}\".");
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    /// <summary>
    /// Tuntutan 4: identitas lama yang masih dipakai endpoint lain tetap tersedia.
    ///
    /// <para>Lima dari tujuh identitas lama pilot <b>bertahan</b> karena endpoint <c>PUT</c> dan
    /// <c>POST</c> aslinya tetap memakainya. Pemegang haknya tidak kehilangan apa pun atas endpoint
    /// tersebut, bahkan sebelum fase perluasan dijalankan.</para>
    /// </summary>
    [Fact]
    public void SurvivingLegacyIdentitiesRemainDeclared()
    {
        var snapshot = Snapshot();

        var missing = SurvivingLegacyIdentities
            .Where(x => !snapshot.DeclaredKeys.Contains(
                PermissionRegistryDescriptor.RegistrySnapshot.Key(x.Resource, x.Action)))
            .Select(x => $"{x.Resource}.{x.Action}")
            .ToList();

        Assert.True(
            missing.Count == 0,
            "Identitas lama berikut hilang dari source padahal endpoint-nya masih ada. " +
            "Pemegang haknya akan kehilangan akses tanpa perluasan: " + string.Join(", ", missing));
    }

    /// <summary>
    /// Tuntutan 4 lanjutan: identitas lama yang benar-benar berhenti dipakai ditutup secara
    /// <b>lunak</b>, bukan dihapus fisik, sehingga <c>SysAccessPolicy</c> yang menunjuknya tetap
    /// utuh dan dapat dipulihkan.
    ///
    /// <para>Dua identitas ini — <c>PatientProcedure.Update</c> dan <c>DoctorQueue.Update</c> —
    /// adalah satu-satunya yang menuntut perluasan hak pada fase berikutnya.</para>
    /// </summary>
    [Fact]
    public async Task RetiredLegacyIdentitiesAreClosedSoftlyAndKeepTheirPolicies()
    {
        await using var dbContext = NewContext();
        var snapshot = Snapshot();

        // Registry awal dibangun dari source saat ini.
        await AccessMenuSeeder.ReconcileAsync(dbContext, snapshot);

        // Identitas yang sudah pensiun sengaja ditanam seolah-olah warisan sebelum pemecahan,
        // lengkap dengan satu hak yang menunjuknya.
        foreach (var (resource, action) in RetiredLegacyIdentities)
        {
            var controller = await dbContext.SysControllerAccesses
                .FirstAsync(x => x.ControllerName == resource);

            var legacyAction = new Models.SysActionAccess
            {
                Id = Guid.NewGuid(),
                ControllerAccessId = controller.Id,
                ActionName = action,
                DisplayName = $"{resource} {action}",
                AccessType = "Update",
                IsActive = true,
                IsDelete = false,
            };

            dbContext.SysActionAccesses.Add(legacyAction);

            dbContext.SysAccessPolicies.Add(new Models.SysAccessPolicy
            {
                Id = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                PositionId = Guid.NewGuid(),
                ControllerAccessId = controller.Id,
                ActionAccessId = legacyAction.Id,
                IsAllowed = true,
                IsActive = true,
            });
        }

        await dbContext.SaveChangesAsync();
        var policiesBefore = await dbContext.SysAccessPolicies.CountAsync();

        // Rekonsiliasi berikutnya menutup identitas yang tidak lagi dideklarasikan source.
        await AccessMenuSeeder.ReconcileAsync(dbContext, snapshot);

        foreach (var (resource, action) in RetiredLegacyIdentities)
        {
            var row = await dbContext.SysActionAccesses
                .Include(x => x.ControllerAccess)
                .FirstOrDefaultAsync(x =>
                    x.ActionName == action &&
                    x.ControllerAccess!.ControllerName == resource);

            Assert.True(row is not null, $"{resource}.{action} dihapus fisik; sejarahnya hilang.");
            Assert.False(row!.IsActive, $"{resource}.{action} seharusnya sudah ditutup.");
            Assert.True(row.IsDelete, $"{resource}.{action} seharusnya ditandai terhapus lunak.");
        }

        Assert.Equal(policiesBefore, await dbContext.SysAccessPolicies.CountAsync());
    }

    /// <summary>
    /// Tuntutan 5: rekonsiliasi registry tidak pernah membuat hak.
    ///
    /// <para>Fase A hanya menyiapkan identitas. Memberi hak tetap tindakan admin, dan perluasan
    /// hak berjalan di luar seeder pada fase terpisah.</para>
    /// </summary>
    [Fact]
    public async Task SplitPreparationNeverCreatesAccessPolicy()
    {
        await using var dbContext = NewContext();

        var result = await AccessMenuSeeder.ReconcileAsync(dbContext, Snapshot());

        Assert.True(result.ActionsUpserted > 0, "Seeder tidak mendaftarkan satu pun action.");
        Assert.Equal(0, await dbContext.SysAccessPolicies.CountAsync());
    }

    /// <summary>
    /// Kemampuan sensitif hasil pemecahan terdaftar, tetapi tidak diberikan kepada siapa pun.
    /// </summary>
    [Fact]
    public async Task SensitiveSplitCapabilitiesAreRegisteredButNeverGranted()
    {
        await using var dbContext = NewContext();
        await AccessMenuSeeder.ReconcileAsync(dbContext, Snapshot());

        foreach (var (resource, action) in new[]
                 {
                     ("PatientProcedure", "Approve"),
                     ("PatientProcedure", "Execute"),
                     ("PatientProcedure", "Cancel"),
                     ("DoctorConsultation", "Complete"),
                     ("DoctorQueue", "FinishConsultation"),
                 })
        {
            var registered = await dbContext.SysActionAccesses
                .AnyAsync(x => x.ActionName == action &&
                               x.ControllerAccess!.ControllerName == resource);

            Assert.True(registered, $"{resource}.{action} tidak terdaftar di registry.");
        }

        Assert.Equal(0, await dbContext.SysAccessPolicies.CountAsync());
    }
}
