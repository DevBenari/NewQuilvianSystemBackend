using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

// =====================================================================================
// Hak akses penanda milik Persetujuan Perjanjian Angsuran HRD.
//
// SATU-SATUNYA deklarasi HrdInstallmentPlanApproval : SelfApprove di seluruh repository. Jangan
// menyalinnya ke AccessMenuSeeder, authorization verifier, atau script SQL mana pun —
// deklarasi ganda menciptakan otoritas penemuan kedua (lihat AccessExplicitPermissionAttribute).
//
// KENAPA PENANDA INI ADA
//
//   HrdInstallmentPlanApprovalController memanggil HasAccessAsync(user, "HrdInstallmentPlanApproval",
//   "SelfApprove") untuk menjawab "orang ini boleh memutuskan perjanjian yang ia ajukan sendiri" —
//   bukan "orang ini boleh memanggil endpoint apa". Approve/Reject tetap izin masing-masing;
//   SelfApprove hanya melonggarkan aturan maker-checker pada pengajuan milik pemutus sendiri.
//
//   Penanda ini membuat kemampuan DAPAT DIBERIKAN lewat Manajemen Role → Akses Role, bukan
//   otomatis diberikan ke peran/akun mana pun. Tidak ada pengecualian berdasarkan nama peran.
//
// CATATAN DEPLOYMENT (lihat FinanceReceivableInstallmentPlanService.IsSelfApprovalPersistenceReadyAsync):
//   Menyetujui pengajuan sendiri baru dapat TERSIMPAN setelah constraint database
//   CK_FinReceivableInstallmentPlan_MakerChecker di-DROP lewat migration DropInstallmentPlanMakerCheckerConstraint; service membaca
//   keberadaan constraint itu dari katalog database, bukan dari flag konfigurasi.
//   Sebelum itu, izin ini hanya berlaku untuk MENOLAK pengajuan sendiri.
//
// Resource HrdInstallmentPlanApproval terdaftar dari HrdInstallmentPlanApprovalController pada
// modul yang sama; penanda yang menunjuk resource tak dikenal ditolak saat registry disusun.
// =====================================================================================

[assembly: AccessExplicitPermission(
    moduleCode: "HUMAN_RESOURCE_BENEFIT_INSTALLMENT_PLAN_APPROVAL",
    resourceName: "HrdInstallmentPlanApproval",
    actionName: "SelfApprove",
    displayName: "Decide Own Installment Plan Request",
    accessType: AccessTypes.Update,
    Description = "Memutuskan (menyetujui/menolak) perjanjian angsuran yang diajukan oleh akun yang sama",
    SortOrder = 6)]
