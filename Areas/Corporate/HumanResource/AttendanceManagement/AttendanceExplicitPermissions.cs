using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda milik Kehadiran HRD (amandemen A52, kontrak v26, HRD-DEC-206/208).
//
// SATU-SATUNYA deklarasi AttendanceReport : ReadAllUnits di seluruh repository. Jangan
// menyalinnya ke AccessMenuSeeder, authorization verifier, atau script SQL mana pun —
// deklarasi ganda menciptakan otoritas penemuan kedua (lihat AccessExplicitPermissionAttribute).
//
// KENAPA PENANDA INI ADA
//
//   HrdUnitScopeService.GetReadScopeAsync memanggil HasAccessAsync(user, "AttendanceReport",
//   "ReadAllUnits") untuk menjawab "orang ini boleh membaca data kehadiran SEMUA unit". Tanpa
//   penanda ini, cakupan baca seseorang hanya unit tempat ia memegang penempatan berjabatan
//   beserta unit di bawahnya.
//
//   Contoh: Sari (Kepala Unit ICU) memegang AttendanceReport : Read tanpa penanda ini, sehingga
//   laporan hanya memuat ICU. Admin HRD diberi penanda ini, sehingga laporan memuat semua unit.
//
//   Penanda ini membuat kemampuannya DAPAT DIBERIKAN lewat Manajemen Role → Akses Role, bukan
//   otomatis diberikan. Tidak satu pun baris SysAccessPolicy dibuat karenanya.
//
// Resource AttendanceReport terdaftar dari AttendanceReportController pada modul yang sama
// (BE-HRD-349); penanda yang menunjuk resource tak dikenal ditolak saat registry disusun.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
    resourceName: "AttendanceReport",
    actionName: "ReadAllUnits",
    displayName: "Read Attendance Data For All Units",
    accessType: AccessTypes.Read,
    Description = "Membaca dashboard, laporan, roster, dan dispensasi seluruh unit, tidak terbatas pada unit penempatan berjabatan",
    SortOrder = 10)]
