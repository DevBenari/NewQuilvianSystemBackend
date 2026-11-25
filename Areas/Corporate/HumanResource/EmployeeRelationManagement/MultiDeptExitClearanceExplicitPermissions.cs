using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda milik Kliring Keluar Lintas Departemen (Menu 12 Clearense).
//
// SATU-SATUNYA deklarasi MultiDeptExitClearance : Signoff<Departemen> di seluruh repository. Jangan
// menyalinnya ke AccessMenuSeeder, authorization verifier, atau script SQL mana pun.
//
// KENAPA PENANDA INI ADA (BE-HRD-365, BE-HRD-372, 20 November 2026)
//
//   Hak endpoint MultiDeptExitClearance : SignoffChecklist menjawab "orang ini boleh menandatangani
//   kliring". Penanda per departemen menjawab "orang ini menandatangani atas nama departemen mana".
//   Tanpa pemisahan ini petugas IT dapat menandatangani pengembalian obat Farmasi atau pelunasan
//   koperasi. Item serah terima unit kerja juga boleh ditandatangani atasan langsung aktif pegawai.
//
//   Penanda membuat kemampuan DAPAT DIBERIKAN lewat Manajemen Role → Akses Role; tidak otomatis
//   diberikan ke peran mana pun dan tidak ada pengecualian berdasarkan nama peran.
//
// Resource MultiDeptExitClearance terdaftar dari MultiDeptExitClearanceController pada modul yang sama.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_EMPLOYEE_RELATION",
    resourceName: "MultiDeptExitClearance",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.MultiDeptExitClearancePermissions.SignoffWorkUnitHandover,
    displayName: "Signoff Work Unit Handover",
    accessType: AccessTypes.Update,
    Description = "Menandatangani item kliring serah terima tugas, kunci, dan dokumen unit kerja",
    SortOrder = 10)]

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_EMPLOYEE_RELATION",
    resourceName: "MultiDeptExitClearance",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.MultiDeptExitClearancePermissions.SignoffGeneralAffairsAsset,
    displayName: "Signoff General Affairs Asset",
    accessType: AccessTypes.Update,
    Description = "Menandatangani item kliring pengembalian aset umum (ID card, seragam, kendaraan)",
    SortOrder = 11)]

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_EMPLOYEE_RELATION",
    resourceName: "MultiDeptExitClearance",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.MultiDeptExitClearancePermissions.SignoffPharmacyMedicalDevice,
    displayName: "Signoff Pharmacy Medical Device",
    accessType: AccessTypes.Update,
    Description = "Menandatangani item kliring obat dan alat kesehatan inventaris",
    SortOrder = 12)]

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_EMPLOYEE_RELATION",
    resourceName: "MultiDeptExitClearance",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.MultiDeptExitClearancePermissions.SignoffFinanceCooperative,
    displayName: "Signoff Finance Cooperative",
    accessType: AccessTypes.Update,
    Description = "Menandatangani item kliring pinjaman koperasi dan kasbon",
    SortOrder = 13)]

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_EMPLOYEE_RELATION",
    resourceName: "MultiDeptExitClearance",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.MultiDeptExitClearancePermissions.SignoffInformationTechnology,
    displayName: "Signoff Information Technology",
    accessType: AccessTypes.Update,
    Description = "Menandatangani item kliring akun SIMRS, email, dan perangkat IT",
    SortOrder = 14)]

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement
{
    /// <summary>Nama aksi penanda hak penandatanganan kliring per departemen.</summary>
    public static class MultiDeptExitClearancePermissions
    {
        public const string WorkUnitHandoverDepartment = "WorkUnitHandover";

        public const string SignoffWorkUnitHandover = "SignoffWorkUnitHandover";
        public const string SignoffGeneralAffairsAsset = "SignoffGeneralAffairsAsset";
        public const string SignoffPharmacyMedicalDevice = "SignoffPharmacyMedicalDevice";
        public const string SignoffFinanceCooperative = "SignoffFinanceCooperative";
        public const string SignoffInformationTechnology = "SignoffInformationTechnology";

        /// <summary>Penanda untuk satu jenis departemen kliring; <c>null</c> bila jenis tidak dikenal.</summary>
        public static string? ForDepartment(string? departmentType) => departmentType switch
        {
            "WorkUnitHandover" => SignoffWorkUnitHandover,
            "GeneralAffairsAsset" => SignoffGeneralAffairsAsset,
            "PharmacyMedicalDevice" => SignoffPharmacyMedicalDevice,
            "FinanceCooperative" => SignoffFinanceCooperative,
            "InformationTechnology" => SignoffInformationTechnology,
            _ => null
        };
    }
}
