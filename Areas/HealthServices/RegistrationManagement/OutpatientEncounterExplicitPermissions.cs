using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda milik Daftar Pasien Rawat Jalan (RJ-DOC-DEC-014).
//
// SATU-SATUNYA deklarasi OutpatientEncounter : ReadAll di seluruh repository. Jangan
// menyalinnya ke AccessMenuSeeder, authorization verifier, atau script SQL mana pun —
// deklarasi ganda menciptakan otoritas penemuan kedua (lihat AccessExplicitPermissionAttribute).
//
// KENAPA PENANDA INI ADA
//
//   ClinicalActorScopeService memanggil HasAccessAsync(user, "OutpatientEncounter", "ReadAll")
//   untuk menjawab "orang ini melihat kunjungan semua klinik" — bukan "orang ini boleh
//   memanggil endpoint apa". Tanpa penanda ini, daftar hanya memuat pasien dokter yang login
//   atau poli cluster perawat yang login.
//
//   Sengaja TIDAK memakai nama role SuperAdmin seperti layar antrean: petugas pendaftaran
//   yang paling sering perlu membatalkan kunjungan menggantung dapat diberi butir ini tanpa
//   menjadi Super Admin.
//
// Resource OutpatientEncounter terdaftar dari OutpatientEncounterController pada modul yang
// sama; penanda yang menunjuk resource tak dikenal ditolak saat registry disusun.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
    resourceName: "OutpatientEncounter",
    actionName: "ReadAll",
    displayName: "Read All Outpatient Encounter",
    accessType: AccessTypes.Read,
    Description = "Melihat kunjungan rawat jalan semua klinik pada Daftar Pasien Rawat Jalan",
    SortOrder = 2)]
