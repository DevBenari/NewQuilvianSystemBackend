namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs
{
    /// <summary>
    /// Saringan laporan HR "Piutang Benefit Pegawai": piutang EMPLOYEE_BENEFIT yang lahir dari AR Karyawan Billing.
    /// Laporan ini HANYA MEMBACA: nilai finansial sepenuhnya milik Finance (FinReceivable) dan hasil potongan gaji
    /// milik jadwal angsuran Finance yang diteruskan ke Payroll. HR tidak punya buku piutang sendiri.
    /// </summary>
    public sealed class EmployeeBenefitReceivableReportQuery
    {
        /// <summary>Nomor piutang, nomor invoice, nama/NIP pemilik benefit, atau nama/No. RM pasien.</summary>
        public string? Search { get; set; }

        /// <summary>FinReceivableStatuses. Kosong = semua kecuali CANCELLED.</summary>
        public string? Status { get; set; }

        /// <summary>OVER_ALLOWANCE / PATIENT_PORTION / MIXED.</summary>
        public string? EmployeeArOrigin { get; set; }

        /// <summary>MstWorkforceProfile.Id pemilik benefit.</summary>
        public Guid? BenefitOwnerWorkforceProfileId { get; set; }

        /// <summary>Hanya piutang yang punya (true) atau belum punya (false) perjanjian angsuran yang disetujui.</summary>
        public bool? HasApprovedInstallmentPlan { get; set; }

        public DateOnly? RecognizedFrom { get; set; }
        public DateOnly? RecognizedTo { get; set; }
        public DateOnly? DueDateFrom { get; set; }
        public DateOnly? DueDateTo { get; set; }

        /// <summary>recognizedAt (bawaan) / dueDate / outstandingAmount / receivableNumber.</summary>
        public string SortBy { get; set; } = "recognizedAt";
        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public sealed class EmployeeBenefitReceivableReportRow
    {
        public Guid ReceivableId { get; set; }
        public string ReceivableNumber { get; set; } = string.Empty;

        /// <summary>Tanggal pengakuan AR di Finance (bukan tanggal handoff Billing).</summary>
        public DateTimeOffset RecognizedAt { get; set; }

        public Guid? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }

        public Guid? BenefitOwnerWorkforceProfileId { get; set; }

        /// <summary>EMPLOYEE atau DOCTOR.</summary>
        public string? BenefitOwnerType { get; set; }

        /// <summary>NIP (karyawan) atau nomor dokter.</summary>
        public string? BenefitOwnerStaffNumber { get; set; }
        public string? BenefitOwnerName { get; set; }

        /// <summary>Pasien pengguna benefit (bisa anggota keluarga).</summary>
        public Guid? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientMedicalRecordNumber { get; set; }
        public string? BenefitRelationship { get; set; }

        /// <summary>OVER_ALLOWANCE / PATIENT_PORTION / MIXED.</summary>
        public string? EmployeeArOrigin { get; set; }

        public decimal OriginalAmount { get; set; }

        /// <summary>Total pembayaran yang sudah dialokasikan Finance ke piutang ini.</summary>
        public decimal AllocatedAmount { get; set; }
        public decimal AdjustedAmount { get; set; }
        public decimal WrittenOffAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateOnly DueDate { get; set; }

        /// <summary>
        /// Total potongan gaji/honor yang BERHASIL (jumlah PaidAmount angsuran pada perjanjian yang disetujui).
        /// Potongan yang gagal tidak dihitung dan tidak mengurangi outstanding.
        /// </summary>
        public decimal PayrollDeductedAmount { get; set; }

        /// <summary>Perjanjian angsuran terbaru yang disetujui/selesai; null bila belum ada.</summary>
        public Guid? InstallmentPlanId { get; set; }
        public string? InstallmentPlanNumber { get; set; }
        public string? InstallmentPlanStatus { get; set; }
        public int? InstallmentCount { get; set; }
        public decimal? InstallmentAmount { get; set; }

        /// <summary>Periode potongan pertama (yyyy-MM).</summary>
        public string? FirstDeductionPeriod { get; set; }

        /// <summary>Periode (yyyy-MM) dan status hasil potongan terakhir yang diterima dari Payroll.</summary>
        public string? LatestDeductionPeriod { get; set; }
        public string? LatestDeductionStatus { get; set; }
        public DateTimeOffset? LatestDeductionResultAt { get; set; }
    }

    public sealed class EmployeeBenefitReceivableReportSummaryResponse
    {
        public int TotalReceivable { get; set; }
        public decimal TotalOriginalAmount { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
        public decimal TotalAllocatedAmount { get; set; }

        /// <summary>Piutang yang sedang dicicil: punya perjanjian angsuran DISETUJUI dan belum lunas.</summary>
        public int UnderInstallmentCount { get; set; }
        public decimal UnderInstallmentOutstandingAmount { get; set; }

        public int SettledCount { get; set; }

        /// <summary>Total potongan gaji/honor yang berhasil.</summary>
        public decimal TotalPayrollDeductedAmount { get; set; }
    }

    public sealed class EmployeeBenefitReceivableInstallmentRow
    {
        public int InstallmentNumber { get; set; }
        public string DeductionPeriod { get; set; } = string.Empty;
        public decimal ScheduledAmount { get; set; }
        public decimal CarriedOverAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset? LastResultAt { get; set; }
    }

    public sealed class EmployeeBenefitReceivablePlanRow
    {
        public Guid PlanId { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int InstallmentCount { get; set; }
        public decimal InstallmentAmount { get; set; }
        public decimal TotalAgreedAmount { get; set; }
        public string FirstDeductionPeriod { get; set; } = string.Empty;
        public DateTimeOffset? ApprovedAt { get; set; }
        public List<EmployeeBenefitReceivableInstallmentRow> Installments { get; set; } = new();
    }

    public sealed class EmployeeBenefitReceivableReportDetailResponse
    {
        public EmployeeBenefitReceivableReportRow Receivable { get; set; } = new();

        /// <summary>Semua perjanjian angsuran piutang ini (termasuk yang menunggu/ditolak/dibatalkan) agar HR melihat riwayatnya.</summary>
        public List<EmployeeBenefitReceivablePlanRow> Plans { get; set; } = new();
    }
}
