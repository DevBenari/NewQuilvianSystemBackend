namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    /// <summary>Sumber satu baris jadwal. Dua sumber tidak pernah digabung berdasarkan kemiripan; identitasnya selalu dibawa.</summary>
    public static class DeductionScheduleSources
    {
        /// <summary>Angsuran perjanjian piutang pegawai di Finance AR (FinReceivableInstallment). Sumber kebenaran piutang.</summary>
        public const string FinanceAr = "FinanceAR";

        /// <summary>Jadwal potongan HRD lama (TrxPayrollDeductionSchedule) yang TIDAK menunjuk perjanjian Finance mana pun.</summary>
        public const string HrdLegacy = "HRDLegacy";
    }

    /// <summary>Cakupan status angsuran pada filter. Selain nilai ini, status angsuran Finance apa adanya.</summary>
    public static class DeductionScheduleScopes
    {
        /// <summary>Belum lunas: perjanjian DISETUJUI dengan angsuran DIJADWALKAN, TERBAYAR_SEBAGIAN, atau TERTUNGGAK.</summary>
        public const string Outstanding = "OUTSTANDING";
    }

    /// <summary>
    /// Filter jadwal angsuran Finance AR (PAY-BE-FIX-029). Hanya membaca; tidak pernah menyinkronkan atau mengubah data Finance.
    /// </summary>
    public class FinanceInstallmentScheduleFilterDto
    {
        /// <summary>MstEmployee.Id (identitas lama). Dicocokkan juga lewat WorkforceProfileId karyawan itu.</summary>
        public Guid? EmployeeId { get; set; }

        /// <summary>MstWorkforceProfile.Id (identitas canonical, termasuk dokter tetap tanpa MstEmployee).</summary>
        public Guid? WorkforceProfileId { get; set; }

        /// <summary>Periode potongan yyyy-MM; kosong = semua periode.</summary>
        public string? Period { get; set; }

        /// <summary>
        /// OUTSTANDING (belum lunas), atau salah satu status angsuran Finance: DIJADWALKAN, TERBAYAR_SEBAGIAN, TERBAYAR, TERTUNGGAK,
        /// DIBATALKAN. Kosong = semua status (termasuk riwayat).
        /// </summary>
        public string? InstallmentStatus { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>Satu angsuran (satu periode) dari perjanjian angsuran piutang pegawai di Finance AR.</summary>
    public class FinanceInstallmentScheduleRowDto
    {
        public string Source { get; set; } = DeductionScheduleSources.FinanceAr;
        public Guid InstallmentId { get; set; }
        public Guid PlanId { get; set; }
        public Guid ReceivableId { get; set; }

        public string PlanNumber { get; set; } = string.Empty;
        public string ReceivableNumber { get; set; } = string.Empty;

        /// <summary>Kode angsuran: nomor perjanjian dan nomor urut (mis. PLN-001-3).</summary>
        public string InstallmentCode { get; set; } = string.Empty;
        public int InstallmentNumber { get; set; }
        public int InstallmentCount { get; set; }
        public string DeductionPeriod { get; set; } = string.Empty;

        public Guid? WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }

        /// <summary>EMPLOYEE atau DOCTOR.</summary>
        public string? OwnerType { get; set; }
        public string? StaffNumber { get; set; }
        public string? StaffName { get; set; }
        public string? OrganizationUnitName { get; set; }

        /// <summary>Nominal terjadwal angsuran ini.</summary>
        public decimal ScheduledAmount { get; set; }

        /// <summary>Tunggakan yang dibawa dari periode sebelumnya.</summary>
        public decimal CarriedOverAmount { get; set; }
        public decimal PaidAmount { get; set; }

        /// <summary>Masih terutang untuk angsuran ini (Scheduled + CarriedOver - Paid), apa adanya dari Finance.</summary>
        public decimal OutstandingAmount { get; set; }

        /// <summary>Saldo piutang (pokok) induk saat ini. Sama untuk semua angsuran satu piutang: jangan dijumlahkan per baris.</summary>
        public decimal ReceivableOutstandingAmount { get; set; }
        public decimal TotalAgreedAmount { get; set; }

        /// <summary>Status perjanjian Finance: DISETUJUI, SELESAI, atau DIBATALKAN.</summary>
        public string AgreementStatus { get; set; } = string.Empty;

        /// <summary>Status angsuran Finance. Tidak dicampur dengan status Payroll Run.</summary>
        public string InstallmentStatus { get; set; } = string.Empty;

        /// <summary>
        /// Keterkaitan dengan Payroll modern bila TERBUKTI ada input potongan untuk angsuran ini; null = belum masuk Payroll Run.
        /// Informasi saja: status angsuran tetap milik Finance.
        /// </summary>
        public string? PayrollRunNumber { get; set; }
        public string? PayrollRunStatus { get; set; }
        public string? PayrollInputStatus { get; set; }
    }

    /// <summary>
    /// Ringkasan atas SELURUH himpunan hasil filter (bukan hanya halaman ini), dihitung dari sumber yang sama dengan tabel.
    /// </summary>
    public class FinanceInstallmentScheduleSummaryDto
    {
        /// <summary>Jumlah angsuran (baris) pada hasil filter.</summary>
        public int InstallmentCount { get; set; }

        /// <summary>Jumlah perjanjian unik.</summary>
        public int AgreementCount { get; set; }

        /// <summary>Jumlah pegawai unik.</summary>
        public int OwnerCount { get; set; }

        /// <summary>Jumlah nominal terjadwal angsuran, tidak termasuk yang DIBATALKAN.</summary>
        public decimal TotalScheduledAmount { get; set; }

        /// <summary>Jumlah masih terutang pada angsuran (TERBAYAR = 0, DIBATALKAN tidak dihitung).</summary>
        public decimal TotalOutstandingInstallment { get; set; }

        /// <summary>Jumlah angsuran TERTUNGGAK.</summary>
        public int OverdueCount { get; set; }

        /// <summary>
        /// Jumlah saldo piutang dari perjanjian DISETUJUI, dihitung SEKALI per piutang unik (satu piutang dengan 12 angsuran tidak
        /// dijumlahkan 12 kali).
        /// </summary>
        public decimal TotalReceivableOutstanding { get; set; }
    }

    /// <summary>Jadwal HRD lama yang tidak menunjuk perjanjian Finance mana pun; disajikan terpisah (granularitas berbeda: per perjanjian).</summary>
    public class LegacyDeductionScheduleRowDto
    {
        public string Source { get; set; } = DeductionScheduleSources.HrdLegacy;
        public Guid Id { get; set; }
        public string ScheduleNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNik { get; set; }
        public string DeductionCategory { get; set; } = string.Empty;
        public int PriorityLevel { get; set; }
        public string? ExternalReferenceId { get; set; }
        public decimal MonthlyInstallmentAmount { get; set; }
        public int TotalInstallmentMonths { get; set; }
        public int PaidInstallmentCount { get; set; }
        public decimal RemainingPrincipal { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ScheduleStatus { get; set; } = string.Empty;
    }

    public class FinanceInstallmentScheduleResponseDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<FinanceInstallmentScheduleRowDto> Items { get; set; } = new();
        public FinanceInstallmentScheduleSummaryDto Summary { get; set; } = new();

        /// <summary>Jadwal HRD lama yang masih relevan dan bukan salinan perjanjian Finance. Kosong bila filter tidak berlaku bagi jadwal lama.</summary>
        public List<LegacyDeductionScheduleRowDto> LegacyItems { get; set; } = new();

        /// <summary>
        /// Jumlah jadwal HRD lama yang disembunyikan karena menunjuk perjanjian Finance yang sama (ExternalReferenceId = nomor perjanjian);
        /// ditampilkan hanya sekali, dari Finance.
        /// </summary>
        public int HiddenLegacyDuplicateCount { get; set; }
    }
}
