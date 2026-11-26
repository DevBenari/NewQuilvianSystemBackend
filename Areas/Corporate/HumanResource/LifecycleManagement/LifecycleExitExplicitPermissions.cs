using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda Serah Terima dan Clearense pada Lifecycle Management (Menu 12).
//
// SATU-SATUNYA deklarasi penanda di bawah ini di seluruh repository. Jangan menyalinnya ke
// AccessMenuSeeder, authorization verifier, atau script SQL mana pun.
//
// KENAPA PENANDA INI ADA (BE-HRD-365, BE-HRD-372, 20 November 2026)
//
//   OffboardingHandoverAdministration : VerifyAllTasks — memverifikasi tugas serah terima pegawai mana
//   pun. Tanpa hak ini, hanya atasan langsung aktif pegawai yang keluar yang boleh memverifikasi.
//
//   ExitClearance : Clear<Aspek> — menandai satu aspek clearance (unit kerja, aset, payroll, akses IT,
//   HR) selesai. Hak endpoint ExitClearance : Update saja tidak lagi cukup untuk mencentang aspek
//   departemen lain.
//
//   Penanda membuat kemampuan DAPAT DIBERIKAN lewat Manajemen Role → Akses Role; tidak ada
//   pengecualian berdasarkan nama peran.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_LIFECYCLE_MANAGEMENT",
    resourceName: "OffboardingHandoverAdministration",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.LifecycleExitPermissions.VerifyAllHandoverTasks,
    displayName: "Verify All Handover Tasks",
    accessType: AccessTypes.Update,
    Description = "Memverifikasi tugas serah terima pegawai mana pun, tidak terbatas pada bawahan langsung",
    SortOrder = 10)]

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_LIFECYCLE_EXIT_CLEARANCE",
    resourceName: "ExitClearance",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.LifecycleExitPermissions.ClearDepartment,
    displayName: "Clear Work Unit Aspect",
    accessType: AccessTypes.Update,
    Description = "Menandai aspek unit kerja exit clearance selesai",
    SortOrder = 20)]

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_LIFECYCLE_EXIT_CLEARANCE",
    resourceName: "ExitClearance",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.LifecycleExitPermissions.ClearAsset,
    displayName: "Clear Asset Aspect",
    accessType: AccessTypes.Update,
    Description = "Menandai aspek pengembalian aset exit clearance selesai",
    SortOrder = 21)]

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_LIFECYCLE_EXIT_CLEARANCE",
    resourceName: "ExitClearance",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.LifecycleExitPermissions.ClearPayroll,
    displayName: "Clear Payroll Aspect",
    accessType: AccessTypes.Update,
    Description = "Menandai aspek payroll exit clearance selesai",
    SortOrder = 22)]

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_LIFECYCLE_EXIT_CLEARANCE",
    resourceName: "ExitClearance",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.LifecycleExitPermissions.ClearItAccess,
    displayName: "Clear IT Access Aspect",
    accessType: AccessTypes.Update,
    Description = "Menandai aspek akses IT exit clearance selesai",
    SortOrder = 23)]

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_LIFECYCLE_EXIT_CLEARANCE",
    resourceName: "ExitClearance",
    actionName: QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.LifecycleExitPermissions.ClearHr,
    displayName: "Clear HR Aspect",
    accessType: AccessTypes.Update,
    Description = "Menandai aspek administrasi HR exit clearance selesai",
    SortOrder = 24)]

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement
{
    /// <summary>Nama aksi penanda hak Serah Terima dan Clearense Lifecycle.</summary>
    public static class LifecycleExitPermissions
    {
        public const string VerifyAllHandoverTasks = "VerifyAllTasks";
        public const string ClearDepartment = "ClearDepartment";
        public const string ClearAsset = "ClearAsset";
        public const string ClearPayroll = "ClearPayroll";
        public const string ClearItAccess = "ClearItAccess";
        public const string ClearHr = "ClearHr";
    }
}
