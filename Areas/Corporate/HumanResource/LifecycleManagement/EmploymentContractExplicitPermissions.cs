using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda Kontrak Kerja (Menu 08).
//
// SATU-SATUNYA deklarasi WorkforceEmploymentContract : ReadAllUnits di seluruh repository. Jangan
// menyalinnya ke AccessMenuSeeder, authorization verifier, atau script SQL mana pun.
//
// KENAPA PENANDA INI ADA (BE-HRD-424, 20 November 2026)
//
//   Hak endpoint WorkforceEmploymentContract : Read menjawab "orang ini boleh membuka layar kontrak".
//   Penanda ReadAllUnits menjawab "orang ini boleh melihat kontrak seluruh unit rumah sakit". Tanpa
//   penanda ini, daftar/detail/cetak/unduh kontrak dibatasi pada kontrak milik sendiri dan pegawai di
//   unit yang dipimpin pengguna beserta unit turunannya.
//
//   Penanda membuat kemampuan DAPAT DIBERIKAN lewat Manajemen Role → Akses Role; tidak ada
//   pengecualian berdasarkan nama peran.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_LIFECYCLE_MANAGEMENT",
    resourceName: "WorkforceEmploymentContract",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.EmploymentContractPermissions.ReadAllUnits,
    displayName: "Read Employment Contracts Of All Units",
    accessType: AccessTypes.Read,
    Description = "Melihat kontrak kerja pegawai seluruh unit, tidak terbatas pada unit yang dipimpin",
    SortOrder = 20)]

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement
{
    /// <summary>Nama aksi penanda hak akses Kontrak Kerja.</summary>
    public static class EmploymentContractPermissions
    {
        public const string ReadAllUnits = "ReadAllUnits";
    }
}
