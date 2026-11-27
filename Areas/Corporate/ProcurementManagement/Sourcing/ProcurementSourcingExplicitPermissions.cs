using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda sub-modul Pemilihan Penyedia (PBJ-PS-PERM 1.0 bagian 1–2; PBJ-DEC-020, PBJ-DEC-079;
// BE-PBJ-PS-005, dipindah dari BE-PBJ-PS-004 karena registry menolak penanda tanpa endpoint pada resource yang sama).
//
// SATU-SATUNYA deklarasi ProcurementPackage : ReadAllUnits dan ProcurementPackage : ReadPrice. Jangan disalin ke
// seeder, verifier, atau script SQL mana pun.
//
// Keduanya bukan izin endpoint, melainkan penyaring data:
// - ReadAllUnits: ProcurementDataScopeService menanyakannya untuk menjawab "pengguna ini melihat paket semua unit atau
//   hanya paket berisi baris PR unit penempatannya". Contoh: Tim Pengadaan dicentang → melihat semua paket; staf IGD
//   tanpa centang → hanya paket yang memuat baris PR IGD.
// - ReadPrice: ProcurementPackageService mengosongkan HPS, harga perkiraan, dan jumlahnya bagi pengguna tanpa centang
//   ini. Contoh: Keuangan memegang ProcurementPackage : Read tanpa ReadPrice → melihat isi paket tanpa HPS.
//
// Penanda ini membuat kemampuannya DAPAT DIBERIKAN lewat layar Akses Role, bukan otomatis diberikan.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "CORPORATE_PROCUREMENT_MANAGEMENT_SOURCING",
    resourceName: "ProcurementPackage",
    actionName: "ReadAllUnits",
    displayName: "Read All Units Procurement Package",
    accessType: AccessTypes.Read,
    Description = "Melihat paket pengadaan seluruh unit, bukan hanya paket berisi permintaan unit penempatan sendiri",
    SortOrder = 10)]

[assembly: AccessExplicitPermission(
    moduleCode: "CORPORATE_PROCUREMENT_MANAGEMENT_SOURCING",
    resourceName: "ProcurementPackage",
    actionName: "ReadPrice",
    displayName: "Read Price Procurement Package",
    accessType: AccessTypes.Read,
    Description = "Melihat HPS, harga perkiraan, dan nilai pada paket pengadaan beserta lampiran harganya",
    SortOrder = 11)]
