using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda sub-modul Permintaan Pengadaan (PBJ-DEC-020, PBJ-DEC-036; BE-PBJ-006).
//
// SATU-SATUNYA deklarasi ProcurementRequest : ReadAllUnits. Jangan disalin ke seeder,
// verifier, atau script SQL mana pun.
//
// Penanda ini bukan izin endpoint, melainkan penyaring data: ProcurementDataScopeService
// menanyakannya untuk menjawab "pengguna ini melihat PR semua unit atau hanya unit
// penempatannya". Contoh: Tim Pengadaan dicentang ReadAllUnits → melihat PR seluruh unit;
// staf IGD tanpa centang → hanya PR IGD. Membuat PR tetap hanya untuk unit penempatan
// sendiri (PBJ-VAL-010), walaupun penanda ini dicentang.
//
// Penanda ini membuat kemampuannya DAPAT DIBERIKAN lewat layar Akses Role, bukan otomatis
// diberikan.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "CORPORATE_PROCUREMENT_MANAGEMENT_REQUEST",
    resourceName: "ProcurementRequest",
    actionName: "ReadAllUnits",
    displayName: "Read All Units Procurement Request",
    accessType: AccessTypes.Read,
    Description = "Melihat permintaan pengadaan seluruh unit, bukan hanya unit penempatan sendiri",
    SortOrder = 5)]
