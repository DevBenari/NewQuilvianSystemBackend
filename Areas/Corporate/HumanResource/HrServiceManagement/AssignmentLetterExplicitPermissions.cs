using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda Surat Tugas (Administrasi Kepegawaian).
//
// SATU-SATUNYA deklarasi AssignmentLetter : ManageAllUnits di seluruh repository. Jangan
// menyalinnya ke AccessMenuSeeder, authorization verifier, atau script SQL mana pun.
//
// KENAPA PENANDA INI ADA (BE-HRD-382/383, 20 November 2026)
//
//   Hak endpoint AssignmentLetter : Create menjawab "orang ini boleh menyusun Surat Tugas".
//   Penanda ManageAllUnits menjawab "orang ini boleh menugaskan dan mengelola Surat Tugas pegawai
//   unit mana pun". Tanpa penanda ini, pengaju (Kepala Unit) hanya boleh memilih pegawai di unit
//   yang dipimpinnya beserta unit turunan, dan hanya mengelola surat yang ia buat.
//
//   Kemampuan DAPAT DIBERIKAN lewat Manajemen Role → Akses Role; tidak ada pengecualian berdasarkan
//   nama peran.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_SERVICE_MANAGEMENT",
    resourceName: "AssignmentLetter",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.AssignmentLetterPermissions.ManageAllUnits,
    displayName: "Manage Assignment Letters Of All Units",
    accessType: AccessTypes.Update,
    Description = "Menugaskan pegawai unit mana pun dan mengelola Surat Tugas yang dibuat orang lain",
    SortOrder = 30)]

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement
{
    /// <summary>Nama aksi penanda hak akses Surat Tugas.</summary>
    public static class AssignmentLetterPermissions
    {
        public const string ManageAllUnits = "ManageAllUnits";
    }
}
