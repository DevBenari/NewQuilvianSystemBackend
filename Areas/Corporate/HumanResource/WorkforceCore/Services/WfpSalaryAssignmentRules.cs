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

        // ------------------------------------------------------------------------------------------------------------------------
        // PAY-BE-FIX-010: status otorisasi dan status operasional.
        //
        // IsApproved/ApprovalStatus di atas hanya menjawab "apakah persetujuan manual tercatat". Keduanya TIDAK menjawab "apakah Payroll
        // boleh memakai penetapan ini", sebab itu bergantung pada kebijakan Payroll:Policy:RequireApprovedSalaryAssignment, status aktif,
        // penetapan utama, dan masa berlaku. Pembaca Payroll (PayrollHrdSourceReader, PayrollCalculationService) sudah memakai aturan itu;
        // bagian ini memberi layar bahasa yang sama tanpa menebak dan tanpa memalsukan ApprovedAt/ApprovedByUserId.
        // ------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Asal wewenang penetapan untuk dipakai Payroll, berdiri sendiri dari tanggal, status aktif, dan penetapan utama:
        /// <see cref="WfpSalaryAuthorizationStatuses.PendingApproval"/> bila kebijakan mensyaratkan persetujuan dan belum tercatat;
        /// <see cref="WfpSalaryAuthorizationStatuses.ManualApproved"/> bila persetujuan manual benar-benar tercatat (kebijakan apa pun);
        /// selain itu <see cref="WfpSalaryAuthorizationStatuses.AutoByPolicy"/>: sah menurut kebijakan tanpa persetujuan manual. AutoByPolicy
        /// bukan persetujuan oleh seseorang dan tidak pernah mengisi ApprovedAt/ApprovedByUserId.
        /// </summary>
        public static string AuthorizationStatusOf(bool approved, bool approvalRequired) =>
            approvalRequired && !approved
                ? WfpSalaryAuthorizationStatuses.PendingApproval
                : approved ? WfpSalaryAuthorizationStatuses.ManualApproved : WfpSalaryAuthorizationStatuses.AutoByPolicy;

        /// <summary>
        /// Keadaan operasional pada <paramref name="today"/>, tanpa memandang asal persetujuan. Urutan: nonaktif, berakhir, belum mulai,
        /// lalu penetapan utama (hanya penetapan utama yang dibayar Payroll; yang lain hanya memunculkan peringatan pada run).
        /// </summary>
        public static string OperationalStatusOf(bool isActive, bool isPrimary, DateTime effectiveStartDate, DateTime? effectiveEndDate, DateTime today)
        {
            if (!isActive) return WfpSalaryOperationalStatuses.Inactive;
            if (effectiveEndDate.HasValue && effectiveEndDate.Value.Date < today.Date) return WfpSalaryOperationalStatuses.Expired;
            if (effectiveStartDate.Date > today.Date) return WfpSalaryOperationalStatuses.Scheduled;
            return isPrimary ? WfpSalaryOperationalStatuses.Current : WfpSalaryOperationalStatuses.NotPrimary;
        }

        /// <summary>
        /// Kedudukan satu penetapan: asal wewenang, keadaan operasional, dan putusan akhir <see cref="SalaryStanding.IsPayrollUsable"/>
        /// (berlaku saat ini sebagai penetapan utama dan bukan menunggu persetujuan). Sama dengan syarat yang dipakai pembaca Payroll.
        /// </summary>
        public static SalaryStanding StandingOf(
            bool isActive,
            bool isPrimary,
            DateTime effectiveStartDate,
            DateTime? effectiveEndDate,
            DateTime? approvedAt,
            Guid? approvedByUserId,
            bool approvalRequired,
            DateTime today)
        {
            var authorization = AuthorizationStatusOf(IsApproved(approvedAt, approvedByUserId), approvalRequired);
            var operational = OperationalStatusOf(isActive, isPrimary, effectiveStartDate, effectiveEndDate, today);
            return new SalaryStanding(
                authorization,
                operational,
                operational == WfpSalaryOperationalStatuses.Current && authorization != WfpSalaryAuthorizationStatuses.PendingApproval);
        }

        /// <summary>Bentuk baku nilai filter status otorisasi (tanpa mengindahkan huruf besar/kecil); null bila bukan nilai yang dikenal.</summary>
        public static string? NormalizeAuthorizationStatus(string? value) =>
            WfpSalaryAuthorizationStatuses.All.FirstOrDefault(x => string.Equals(x, value?.Trim(), StringComparison.OrdinalIgnoreCase));

        /// <summary>Bentuk baku nilai filter status operasional (tanpa mengindahkan huruf besar/kecil); null bila bukan nilai yang dikenal.</summary>
        public static string? NormalizeOperationalStatus(string? value) =>
            WfpSalaryOperationalStatuses.All.FirstOrDefault(x => string.Equals(x, value?.Trim(), StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Padanan <see cref="AuthorizationStatusOf"/> untuk query EF (diterjemahkan ke SQL). Nilai harus sudah dinormalkan lewat
        /// <see cref="NormalizeAuthorizationStatus"/>. Ketika kebijakan TIDAK mensyaratkan persetujuan, tidak ada baris yang berstatus
        /// PendingApproval; ketika mensyaratkan, tidak ada baris yang berstatus AutoByPolicy.
        /// </summary>
        public static Expression<Func<WfpSalaryAssignment, bool>> AuthorizationStatusExpression(string status, bool approvalRequired)
        {
            if (status == WfpSalaryAuthorizationStatuses.ManualApproved) return IsApprovedExpression;
            if (status == WfpSalaryAuthorizationStatuses.PendingApproval)
            {
                if (approvalRequired) return IsPendingExpression;
                return x => false;
            }

            // AutoByPolicy: belum ada persetujuan manual dan kebijakan tidak mensyaratkannya.
            if (approvalRequired) return x => false;
            return IsPendingExpression;
        }

        /// <summary>
        /// Padanan <see cref="OperationalStatusOf"/> untuk query EF (diterjemahkan ke SQL). Nilai harus sudah dinormalkan lewat
        /// <see cref="NormalizeOperationalStatus"/>. Perbandingan memakai tanggal acuan <paramref name="today"/> yang sama dengan pembacaan.
        /// </summary>
        public static Expression<Func<WfpSalaryAssignment, bool>> OperationalStatusExpression(string status, DateTime today)
        {
            var day = today.Date;
            if (status == WfpSalaryOperationalStatuses.Inactive) return x => !x.IsActive;
            if (status == WfpSalaryOperationalStatuses.Expired)
                return x => x.IsActive && x.EffectiveEndDate != null && x.EffectiveEndDate < day;
            if (status == WfpSalaryOperationalStatuses.Scheduled)
                return x => x.IsActive && x.EffectiveStartDate > day && !(x.EffectiveEndDate != null && x.EffectiveEndDate < day);
            if (status == WfpSalaryOperationalStatuses.NotPrimary)
                return x => x.IsActive && !x.IsPrimary && x.EffectiveStartDate <= day && (x.EffectiveEndDate == null || x.EffectiveEndDate >= day);

            // Current: aktif, utama, dan masa berlaku mencakup tanggal acuan.
            return x => x.IsActive && x.IsPrimary && x.EffectiveStartDate <= day && (x.EffectiveEndDate == null || x.EffectiveEndDate >= day);
        }

        /// <summary>
        /// Padanan <see cref="SalaryStanding.IsPayrollUsable"/> untuk query EF: berlaku saat ini sebagai penetapan utama dan, hanya bila
        /// kebijakan mensyaratkan persetujuan, persetujuan manualnya tercatat.
        /// </summary>
        public static Expression<Func<WfpSalaryAssignment, bool>> PayrollUsableExpression(bool approvalRequired, DateTime today)
        {
            var day = today.Date;
            if (approvalRequired)
                return x => x.IsActive && x.IsPrimary && x.EffectiveStartDate <= day && (x.EffectiveEndDate == null || x.EffectiveEndDate >= day)
                    && x.ApprovedAt != null && x.ApprovedByUserId != null;

            return x => x.IsActive && x.IsPrimary && x.EffectiveStartDate <= day && (x.EffectiveEndDate == null || x.EffectiveEndDate >= day);
        }

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

    /// <summary>
    /// Asal wewenang penetapan gaji untuk dipakai Payroll (PAY-BE-FIX-010). Nilai ini adalah KUNCI BAKU yang dibaca Frontend; jangan
    /// diterjemahkan. <see cref="AutoByPolicy"/> berarti sah menurut kebijakan Payroll tanpa persetujuan manual: ia bukan persetujuan oleh
    /// seseorang dan tidak mengisi ApprovedAt/ApprovedByUserId.
    /// </summary>
    public static class WfpSalaryAuthorizationStatuses
    {
        public const string ManualApproved = "ManualApproved";
        public const string AutoByPolicy = "AutoByPolicy";
        public const string PendingApproval = WfpSalaryAssignmentRules.PendingApproval;

        public static readonly IReadOnlyList<string> All = new[] { ManualApproved, AutoByPolicy, PendingApproval };
    }

    /// <summary>
    /// Keadaan operasional penetapan gaji pada tanggal acuan, terpisah dari asal persetujuannya (PAY-BE-FIX-010). Tidak ada nilai Conflict:
    /// dua penetapan utama aktif tidak dapat ada bersamaan (indeks unik IX_WfpSalaryAssignment_WorkforceProfileId dan penolakan irisan pada
    /// setiap penulisan), sehingga keadaan itu tidak dapat muncul dari data.
    /// </summary>
    public static class WfpSalaryOperationalStatuses
    {
        public const string Current = "Current";
        public const string Scheduled = "Scheduled";
        public const string Expired = "Expired";
        public const string Inactive = "Inactive";
        public const string NotPrimary = "NotPrimary";

        public static readonly IReadOnlyList<string> All = new[] { Current, Scheduled, Expired, Inactive, NotPrimary };
    }

    /// <summary>Kedudukan satu penetapan gaji terhadap Payroll pada tanggal acuan (PAY-BE-FIX-010). Murni turunan; tidak disimpan.</summary>
    public readonly record struct SalaryStanding(string AuthorizationStatus, string OperationalStatus, bool IsPayrollUsable);
}
