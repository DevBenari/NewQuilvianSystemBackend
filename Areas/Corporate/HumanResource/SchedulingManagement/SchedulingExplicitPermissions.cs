using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda milik Penjadwalan HRD (amandemen A52, kontrak v26, HRD-DEC-194/206/208).
//
// SATU-SATUNYA deklarasi RosterPeriod : ManageAllUnits di seluruh repository. Jangan
// menyalinnya ke AccessMenuSeeder, authorization verifier, atau script SQL mana pun —
// deklarasi ganda menciptakan otoritas penemuan kedua (lihat AccessExplicitPermissionAttribute).
//
// KENAPA PENANDA INI ADA
//
//   HrdUnitScopeService.GetManageScopeAsync memanggil HasAccessAsync(user, "RosterPeriod",
//   "ManageAllUnits") untuk menjawab "orang ini boleh mengelola roster SEMUA unit" — bukan
//   "orang ini boleh memanggil endpoint apa". Tanpa penanda ini, cakupan kelola seseorang
//   hanya unit tempat ia memegang penempatan berjabatan beserta unit di bawahnya.
//
//   Contoh: Sari (Kepala Ruang ICU) memegang RosterPeriod : Update tanpa penanda ini, sehingga
//   hanya boleh mengubah roster ICU. Petugas HR Scheduling diberi penanda ini, sehingga boleh
//   mengubah roster unit mana pun.
//
//   Penanda ini membuat kemampuannya DAPAT DIBERIKAN lewat Manajemen Role → Akses Role, bukan
//   otomatis diberikan. Tidak satu pun baris SysAccessPolicy dibuat karenanya.
//
// PENANDA KEDUA YANG BELUM DIDEKLARASIKAN
//
//   AttendanceReport : ReadAllUnits (cakupan baca semua unit) sengaja BELUM ditulis di sini.
//   Resource AttendanceReport baru lahir dari AttendanceReportController (BE-HRD-349), dan
//   registry menolak keras penanda yang menunjuk resource yang belum terdaftar. Deklarasikan
//   bersama controller itu, di berkas penanda modul HUMAN_RESOURCE_ATTENDANCE.
//
// Resource RosterPeriod terdaftar dari RosterPeriodController pada modul yang sama; penanda yang
// menunjuk resource tak dikenal ditolak saat registry disusun.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_SCHEDULING_MANAGEMENT",
    resourceName: "RosterPeriod",
    actionName: "ManageAllUnits",
    displayName: "Manage Roster For All Units",
    accessType: AccessTypes.Update,
    Description = "Mengelola roster seluruh unit, tidak terbatas pada unit penempatan berjabatan",
    SortOrder = 10)]
