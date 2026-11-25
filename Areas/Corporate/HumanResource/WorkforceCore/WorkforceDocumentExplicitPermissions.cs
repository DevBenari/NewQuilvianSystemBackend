using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda milik Dokumen Personal Pegawai (Menu 04).
//
// SATU-SATUNYA deklarasi WorkforceDocument : ViewConfidential di seluruh repository. Jangan
// menyalinnya ke AccessMenuSeeder, authorization verifier, atau script SQL mana pun.
//
// KENAPA PENANDA INI ADA (BE-HRD-397, HRD-DEC-053, 20 November 2026)
//
//   WfpDocumentController memanggil HasAccessAsync(user, "WorkforceDocument", "ViewConfidential")
//   untuk menjawab "orang ini boleh membuka dokumen personal bertanda rahasia milik pegawai lain"
//   — bukan "orang ini boleh memanggil endpoint apa". Pemilik dokumen tetap selalu boleh membuka
//   dokumennya sendiri. Sebelumnya kemampuan ini ditentukan dari nama peran yang ditulis langsung
//   di kode (SuperAdmin, Admin, HRAdmin, HRManager), dan itu dilarang role-access-rules.
//
//   Penanda ini membuat kemampuan DAPAT DIBERIKAN lewat Manajemen Role → Akses Role, bukan
//   otomatis diberikan ke peran/akun mana pun.
//
// Resource WorkforceDocument terdaftar dari WfpDocumentController pada modul yang sama.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
    resourceName: "WorkforceDocument",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.WorkforceDocumentPermissions.ViewConfidential,
    displayName: "View Confidential Workforce Document",
    accessType: AccessTypes.Read,
    Description = "Melihat detail dan mengunduh dokumen personal bertanda rahasia milik pegawai lain",
    SortOrder = 6)]

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore
{
    /// <summary>Nama aksi penanda hak akses Dokumen Personal Pegawai yang diperiksa di dalam kode.</summary>
    public static class WorkforceDocumentPermissions
    {
        public const string ViewConfidential = "ViewConfidential";
    }
}
