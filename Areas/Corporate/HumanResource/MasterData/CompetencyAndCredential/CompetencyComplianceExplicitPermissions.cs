using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda Matriks Kepatuhan Persyaratan Jabatan (Menu 09).
//
// SATU-SATUNYA deklarasi Competency : ReadAllUnitsCompliance di seluruh repository. Jangan
// menyalinnya ke AccessMenuSeeder, authorization verifier, atau script SQL mana pun.
//
// KENAPA PENANDA INI ADA (BE-HRD-429, 20 November 2026)
//
//   Hak endpoint Competency : Read menjawab "orang ini boleh membuka matriks kepatuhan". Penanda
//   ReadAllUnitsCompliance menjawab "orang ini boleh mengevaluasi pegawai seluruh unit". Tanpa
//   penanda ini, evaluasi per pegawai dibatasi pada diri sendiri dan pegawai di unit yang dipimpin
//   pengguna beserta unit turunannya (Kepala Unit tidak melihat unit lain).
//
//   Penanda membuat kemampuan DAPAT DIBERIKAN lewat Manajemen Role → Akses Role; tidak ada
//   pengecualian berdasarkan nama peran.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
    resourceName: "Competency",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.CompetencyCompliancePermissions.ReadAllUnitsCompliance,
    displayName: "Read Position Compliance Of All Units",
    accessType: AccessTypes.Read,
    Description = "Mengevaluasi matriks kepatuhan persyaratan jabatan pegawai seluruh unit",
    SortOrder = 30)]

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential
{
    /// <summary>Nama aksi penanda hak akses matriks kepatuhan persyaratan jabatan.</summary>
    public static class CompetencyCompliancePermissions
    {
        public const string ReadAllUnitsCompliance = "ReadAllUnitsCompliance";
    }
}
