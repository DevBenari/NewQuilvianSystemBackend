using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda Penerbitan Dokumen Kepegawaian (Surat Keterangan).
//
// SATU-SATUNYA deklarasi EmployeeDocumentIssuance : AccessAllEmployees di seluruh repository.
// Jangan menyalinnya ke AccessMenuSeeder, authorization verifier, atau script SQL mana pun.
//
// KENAPA PENANDA INI ADA (BE-HRD-381, 20 November 2026)
//
//   Hak endpoint EmployeeDocumentIssuance : Request / Download menjawab "orang ini boleh memakai
//   fitur surat". Penanda AccessAllEmployees menjawab "orang ini boleh mengajukan dan mengunduh
//   surat untuk pegawai mana pun". Tanpa penanda ini, pengaju hanya boleh memohon untuk dirinya
//   sendiri dan pegawai di unit yang dipimpinnya (beserta unit turunan), dan hanya boleh mengunduh
//   surat miliknya atau yang ia terbitkan.
//
//   Menggantikan pemeriksaan nama peran tertanam (SuperAdmin/HRAdmin/HRManager/HRStaff) pada unduhan.
//   Kemampuan DAPAT DIBERIKAN lewat Manajemen Role → Akses Role.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_SERVICE_MANAGEMENT",
    resourceName: "EmployeeDocumentIssuance",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.EmployeeDocumentIssuancePermissions.AccessAllEmployees,
    displayName: "Manage Employee Letters For All Employees",
    accessType: AccessTypes.Read,
    Description = "Mengajukan dan mengunduh surat kepegawaian untuk pegawai mana pun (bukan hanya diri sendiri/unit yang dipimpin)",
    SortOrder = 20)]

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement
{
    /// <summary>Nama aksi penanda hak akses Penerbitan Dokumen Kepegawaian.</summary>
    public static class EmployeeDocumentIssuancePermissions
    {
        public const string AccessAllEmployees = "AccessAllEmployees";
    }
}
