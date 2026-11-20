using System.Linq.Expressions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    /// <summary>
    /// Satu-satunya definisi "penetapan gaji disetujui" dan irisan masa berlaku untuk <see cref="WfpSalaryAssignment"/> (PAY-BE-FIX-008).
    /// Sebelumnya tiga pembaca memakai tiga arti berbeda (IsActive, ApprovedAt saja, ApprovedAt dan ApprovedByUserId), sehingga
    /// penetapan yang hanya aktif tetapi belum disetujui dapat dibaca sebagai gaji sah. Murni: tanpa akses data dan tanpa efek samping.
    ///
    /// Disetujui = ApprovedAt dan ApprovedByUserId sama-sama terisi. Aktif (IsActive) HANYA berarti baris tidak dinonaktifkan; ia tidak
    /// pernah berarti disetujui. Skema saat ini tidak punya status Draft terpisah dari Menunggu Persetujuan, jadi hanya dua status yang
    /// dapat dibuktikan dari data: <see cref="Approved"/> dan <see cref="PendingApproval"/>.
    /// </summary>
    public static class WfpSalaryAssignmentRules
    {
        public const string Approved = "Approved";
        public const string PendingApproval = "PendingApproval";

        /// <summary>Syarat disetujui untuk dipakai di query EF (diterjemahkan ke SQL).</summary>
        public static readonly Expression<Func<WfpSalaryAssignment, bool>> IsApprovedExpression =
            x => x.ApprovedAt != null && x.ApprovedByUserId != null;

        /// <summary>Kebalikan <see cref="IsApprovedExpression"/>: belum disetujui atau persetujuannya dibatalkan.</summary>
        public static readonly Expression<Func<WfpSalaryAssignment, bool>> IsPendingExpression =
            x => x.ApprovedAt == null || x.ApprovedByUserId == null;

        public static bool IsApproved(DateTime? approvedAt, Guid? approvedByUserId) =>
            approvedAt.HasValue && approvedByUserId.HasValue;

        public static string StatusOf(DateTime? approvedAt, Guid? approvedByUserId) =>
            IsApproved(approvedAt, approvedByUserId) ? Approved : PendingApproval;

        /// <summary>Dua masa berlaku beririsan bila keduanya berbagi sedikitnya satu hari; batas akhir kosong berarti tidak berakhir.</summary>
        public static bool Overlaps(DateTime startA, DateTime? endA, DateTime startB, DateTime? endB) =>
            startA.Date <= (endB ?? DateTime.MaxValue).Date && startB.Date <= (endA ?? DateTime.MaxValue).Date;

        /// <summary>
        /// Perubahan yang mengubah hak finansial penetapan: struktur, grade, gaji pokok, mata uang, frekuensi, dan tanggal efektif.
        /// Perubahan ini membatalkan persetujuan lama. Status utama/aktif punya endpoint dan penjaganya sendiri; keterangan, penanda
        /// kerahasiaan, grade pegawai, dan periode payroll tidak mengubah hak finansial.
        /// </summary>
        public static bool HasFinancialChange(
            WfpSalaryAssignment current,
            Guid salaryStructureId,
            Guid salaryGradeId,
            decimal baseSalary,
            string currencyCode,
            string paymentFrequency,
            DateTime effectiveStartDate,
            DateTime? effectiveEndDate) =>
            current.SalaryStructureId != salaryStructureId
            || current.SalaryGradeId != salaryGradeId
            || current.BaseSalary != baseSalary
            || !string.Equals(current.CurrencyCode, currencyCode, StringComparison.Ordinal)
            || !string.Equals(current.PaymentFrequency, paymentFrequency, StringComparison.Ordinal)
            || current.EffectiveStartDate.Date != effectiveStartDate.Date
            || current.EffectiveEndDate?.Date != effectiveEndDate?.Date;

        /// <summary>
        /// Penutupan masa berlaku: satu-satunya perubahan finansial adalah batas akhir yang diisi atau dimajukan. Penutupan tidak
        /// membatalkan persetujuan karena tidak menambah hak apa pun; ia hanya menghentikan hak yang sudah disetujui lebih awal.
        /// </summary>
        public static bool IsClosureOnly(
            WfpSalaryAssignment current,
            Guid salaryStructureId,
            Guid salaryGradeId,
            decimal baseSalary,
            string currencyCode,
            string paymentFrequency,
            DateTime effectiveStartDate,
            DateTime? effectiveEndDate)
        {
            if (current.SalaryStructureId != salaryStructureId
                || current.SalaryGradeId != salaryGradeId
                || current.BaseSalary != baseSalary
                || !string.Equals(current.CurrencyCode, currencyCode, StringComparison.Ordinal)
                || !string.Equals(current.PaymentFrequency, paymentFrequency, StringComparison.Ordinal)
                || current.EffectiveStartDate.Date != effectiveStartDate.Date)
                return false;

            if (!effectiveEndDate.HasValue) return false;
            return !current.EffectiveEndDate.HasValue || effectiveEndDate.Value.Date < current.EffectiveEndDate.Value.Date;
        }
    }
}
