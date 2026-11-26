using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda milik Delegasi Wewenang (Plh).
//
// SATU-SATUNYA deklarasi WorkflowDelegation : ViewAllDelegations di seluruh repository. Jangan
// menyalinnya ke AccessMenuSeeder, authorization verifier, atau script SQL mana pun.
//
// KENAPA PENANDA INI ADA (register-masalah-hr.md MSL-05, 18 November 2026)
//
//   WorkflowDelegationsController memanggil HasAccessAsync(user, "WorkflowDelegation",
//   "ViewAllDelegations") untuk menjawab "orang ini boleh melihat dan mencabut surat kuasa Plh
//   seluruh unit" — bukan "orang ini boleh memanggil endpoint apa". Tanpa hak ini, daftar delegasi
//   hanya memuat surat kuasa tempat pegawai login menjadi pemberi atau penerima kuasa, dan pencabutan
//   hanya boleh oleh pemberi kuasa.
//
//   Penanda ini membuat kemampuan DAPAT DIBERIKAN lewat Manajemen Role → Akses Role, bukan
//   otomatis diberikan ke peran/akun mana pun. Tidak ada pengecualian berdasarkan nama peran.
//
// Resource WorkflowDelegation terdaftar dari WorkflowDelegationsController pada modul yang sama.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_WORKFLOW",
    resourceName: "WorkflowDelegation",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.WorkflowDelegationPermissions.ViewAllDelegations,
    displayName: "View And Revoke All Delegations",
    accessType: AccessTypes.Read,
    Description = "Melihat dan mencabut surat kuasa Plh seluruh unit, tidak terbatas pada surat kuasa milik sendiri",
    SortOrder = 5)]

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement
{
    /// <summary>Nama aksi penanda hak akses Delegasi Wewenang yang diperiksa di dalam kode.</summary>
    public static class WorkflowDelegationPermissions
    {
        public const string ViewAllDelegations = "ViewAllDelegations";
    }
}
