using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda milik Evaluasi Kandidat (Tes/Wawancara, Paket B kontrak v30).
//
// SATU-SATUNYA deklarasi CandidateEvaluation : ViewAll di seluruh repository. Jangan menyalinnya ke
// AccessMenuSeeder, authorization verifier, atau script SQL mana pun.
//
// KENAPA PENANDA INI ADA (BE-HRD-454, HRD-DEC-241, HRD-ASM-98, 23 November 2026)
//
//   Hak endpoint CandidateEvaluation : Read menjawab "orang ini boleh membuka daftar Tes/Wawancara".
//   Tanpa penanda ini, daftar dan detail hanya memuat evaluasi yang EvaluatorUserId-nya pengguna itu
//   sendiri, sehingga asesor unit tidak dapat membaca penilaian kandidat unit lain. Recruiter HR dan
//   Manajer SDM yang perlu melihat semua evaluasi diberi penanda ini.
//
//   Penanda TIDAK membuka kirim nilai uji klinis milik penilai lain (VR-PKB-08).
//
//   Penanda membuat kemampuan DAPAT DIBERIKAN lewat Manajemen Role → Akses Role; tidak otomatis
//   diberikan ke peran mana pun dan tidak ada pengecualian berdasarkan nama peran.
//
// Resource CandidateEvaluation terdaftar dari CandidateEvaluationController pada modul yang sama.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_RECRUITMENT",
    resourceName: "CandidateEvaluation",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.RecruitmentEvaluationPermissions.ViewAll,
    displayName: "View All Evaluations",
    accessType: AccessTypes.Read,
    Description = "Melihat seluruh evaluasi kandidat, bukan hanya yang ditugaskan kepada diri sendiri",
    SortOrder = 10)]

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement
{
    /// <summary>Nama aksi penanda hak akses evaluasi kandidat.</summary>
    public static class RecruitmentEvaluationPermissions
    {
        public const string Resource = "CandidateEvaluation";
        public const string ViewAll = "ViewAll";
    }
}
