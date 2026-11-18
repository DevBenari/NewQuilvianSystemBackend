using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda sub-modul Perencanaan Pengadaan (PBJ-DEC-020, PBJ-DEC-036).
//
// SATU-SATUNYA deklarasi <resource perencanaan> : ReadAllUnits. Jangan disalin ke seeder,
// verifier, atau script SQL mana pun. Setiap baris didaftarkan task yang membuat controller
// dokumennya: ProcurementNeedPlan (BE-PBJ-009), ProcurementBudget (BE-PBJ-010), ProcurementPackagePlan
// (BE-PBJ-011).
//
// Penanda ini bukan izin endpoint, melainkan penyaring data: ProcurementDataScopeService
// menanyakannya untuk menjawab "pengguna ini melihat dokumen semua unit atau hanya unit
// penempatannya". Contoh: Tim Pengadaan dicentang ProcurementNeedPlan : ReadAllUnits → melihat
// rencana seluruh unit; penyusun IGD tanpa centang → hanya rencana IGD. Membuat rencana tetap
// hanya untuk unit penempatan sendiri.
//
// Penanda ini membuat kemampuannya DAPAT DIBERIKAN lewat layar Akses Role, bukan otomatis
// diberikan.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "CORPORATE_PROCUREMENT_MANAGEMENT_PLANNING",
    resourceName: "ProcurementNeedPlan",
    actionName: "ReadAllUnits",
    displayName: "Read All Units Procurement Need Plan",
    accessType: AccessTypes.Read,
    Description = "Melihat rencana kebutuhan seluruh unit, bukan hanya unit penempatan sendiri",
    SortOrder = 6)]

[assembly: AccessExplicitPermission(
    moduleCode: "CORPORATE_PROCUREMENT_MANAGEMENT_PLANNING",
    resourceName: "ProcurementBudget",
    actionName: "ReadAllUnits",
    displayName: "Read All Units Procurement Budget",
    accessType: AccessTypes.Read,
    Description = "Melihat pagu seluruh unit, bukan hanya unit penempatan sendiri",
    SortOrder = 6)]

[assembly: AccessExplicitPermission(
    moduleCode: "CORPORATE_PROCUREMENT_MANAGEMENT_PLANNING",
    resourceName: "ProcurementPackagePlan",
    actionName: "ReadAllUnits",
    displayName: "Read All Units Procurement Package Plan",
    accessType: AccessTypes.Read,
    Description = "Melihat konsolidasi dan rencana paket seluruh unit, bukan hanya unit penempatan sendiri",
    SortOrder = 6)]
