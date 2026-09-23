using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda milik modul Radiologi.
//
// SATU-SATUNYA deklarasi RadReport : ActAsRadiologist di seluruh repository.
//
// Jangan menyalinnya ke AccessMenuSeeder, ke authorization verifier, ke
// required-identities.txt, atau ke script SQL mana pun. Deklarasi ganda menciptakan
// otoritas penemuan kedua: seeder dan verifier akan melihat registry yang berbeda, dan
// selisihnya muncul selamanya sebagai DB_ONLY_ACTIVE pada audit drift.
//
// Dibaca PermissionRegistryDescriptor lewat ReadExplicitPermissions, yang dipanggil
// kedua entry point — Build(provider) yang dipakai AccessMenuSeeder, dan
// BuildFromAssembly(assembly) yang dipakai authorization verifier. Keduanya bermuara
// pada BuildCore yang sama, sehingga registry keduanya identik.
//
// KENAPA PENANDA INI ADA
//
//   RadReportService.HasRadiologistAuthorityAsync memanggil
//   HasAccessAsync(user, "RadReport", "ActAsRadiologist") untuk menjawab "orang ini
//   dihitung sebagai dokter radiolog" — bukan "orang ini boleh memanggil endpoint apa".
//   Tidak ada action controller yang pantas menampungnya.
//
//   Sengaja TIDAK digabung dengan RadReport : Validate (RAD-DEC-003, RAD-DEC-015).
//   "Boleh mencoba mengesahkan" dan "dihitung sebagai radiolog" adalah dua kewenangan
//   berbeda: seorang residen dapat diberi Validate supaya dapat mengesahkan draf
//   radiografer, dan tanpa penanda ini draf yang ia tulis sendiri tetap ditolak.
//   Menggabungkan keduanya menghapus aturan itu.
//
// Penanda ini membuat kemampuannya DAPAT DIBERIKAN, bukan otomatis diberikan: ia hanya
// menambah baris SysActionAccess supaya dapat dicentang pada layar Akses Role. Tidak
// satu pun baris SysAccessPolicy dibuat karenanya.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HEALTH_SERVICE_RADIOLOGY_MANAGEMENT",
    resourceName: "RadReport",
    actionName: "ActAsRadiologist",
    displayName: "Act As Radiologist",
    accessType: AccessTypes.Update,
    Description = "Dihitung sebagai dokter radiolog saat menulis dan mengesahkan hasil bacaan",
    SortOrder = 7)]
