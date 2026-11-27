using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda Kasus Onboarding Pegawai (FIN-HRD-ONB-WIZ-01/02).
//
// Penanda ReadAllUnits menjawab "pengguna boleh membaca kasus onboarding seluruh unit
// rumah sakit". Tanpa penanda ini (dan tanpa kewenangan HR DeclareReady/Reassign/CancelJoin),
// pengguna hanya melihat kasus unit yang dipimpinnya beserta turunannya.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_LIFECYCLE_MANAGEMENT",
    resourceName: "WorkforceOnboardingCase",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.OnboardingCasePermissions.ReadAllUnits,
    displayName: "Read Onboarding Cases Of All Units",
    accessType: AccessTypes.Read,
    Description = "Melihat seluruh kasus onboarding pegawai lintas unit rumah sakit",
    SortOrder = 21)]

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement
{
    /// <summary>Nama aksi penanda hak akses Kasus Onboarding.</summary>
    public static class OnboardingCasePermissions
    {
        public const string ReadAllUnits = "ReadAllUnits";
    }
}
