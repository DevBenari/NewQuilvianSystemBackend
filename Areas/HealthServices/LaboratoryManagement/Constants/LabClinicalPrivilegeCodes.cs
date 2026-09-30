using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Constants
{
    /// <summary>
    /// Kode kewenangan klinis pada kredensial Human Resource (<c>WfpClinicalPrivilege.PrivilegeCode</c>)
    /// yang menunjuk seseorang memvalidasi atau merilis hasil laboratorium (<c>INT-07</c>,
    /// <c>LAB-DEC-148</c>).
    ///
    /// <b>Nilainya masih usulan</b> dan baru final lewat <c>LAB-COORD-016</c> bersama pemilik
    /// Human Resource. Selama katalog Human Resource belum memuat kode yang sama persis, setiap
    /// validasi dan rilis ditolak <c>NotAppointed</c> — itu perilaku yang benar, bukan kerusakan.
    ///
    /// <b>Konstanta, bukan data</b> (<c>02-backend-architecture.md</c> 20.9): kode yang dapat
    /// diubah dari layar akan membuat siapa pun pemegang hak ubah pengaturan dapat mengarahkan
    /// validasi ke kode yang ia pegang sendiri. Mengubahnya lewat tinjauan kode.
    /// </summary>
    public static class LabClinicalPrivilegeCodes
    {
        /// <summary>Validasi hasil Patologi Klinik.</summary>
        public const string ValidationClinicalPathology = "LAB-VAL-PK";

        /// <summary>Rilis hasil Patologi Klinik.</summary>
        public const string ReleaseClinicalPathology = "LAB-REL-PK";

        /// <summary>
        /// Kode yang dicari untuk satu disiplin dan satu jenis tindakan.
        ///
        /// Mengembalikan <c>null</c> bagi disiplin yang belum punya kode — hari ini Mikrobiologi
        /// (<c>S4d</c>) dan Patologi Anatomi (<c>S4e</c>). Resolver lalu menolak
        /// <c>NotAppointed</c>, sehingga seandainya penjaga disiplin pada service terlewat, tidak
        /// ada jalan pintas (fail-closed berlapis).
        /// </summary>
        public static string? For(LabDiscipline discipline, LabPrivilegeKind kind) => (discipline, kind) switch
        {
            (LabDiscipline.ClinicalPathology, LabPrivilegeKind.Validation) => ValidationClinicalPathology,
            (LabDiscipline.ClinicalPathology, LabPrivilegeKind.Release) => ReleaseClinicalPathology,
            _ => null
        };
    }
}
