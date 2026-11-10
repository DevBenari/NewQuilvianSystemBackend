using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs
{
    // OPT-API-v1 grup "Health Services / Optical Management / Opt Report" (BE-OPT-052, OPT-DEC-025).

    /// <summary>Periode laporan dalam tanggal WIB, termasuk kedua ujung. Kosong = awal bulan berjalan s.d. hari ini.</summary>
    public class OptReportPeriodQuery
    {
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
    }

    /// <summary>Tanggal acuan laporan pesanan terbuka (WIB). Kosong = hari ini.</summary>
    public class OptOpenOrderReportQuery
    {
        public DateOnly? AsOf { get; set; }
    }

    // ---------------------------------------------------------------- (1) kunjungan per pemeriksa

    public class OptVisitReportResponse
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }

        /// <summary>Kunjungan unit Optik pada periode (tanggal kunjungan WIB), tidak termasuk yang dibatalkan.</summary>
        public int TotalVisits { get; set; }

        /// <summary>Pemeriksaan refraksi yang difinalkan pada periode.</summary>
        public int TotalExams { get; set; }

        public List<OptVisitReportRow> Examiners { get; set; } = new();
    }

    public class OptVisitReportRow
    {
        public Guid ExaminerWorkforceProfileId { get; set; }
        public string ExaminerName { get; set; } = string.Empty;

        /// <summary>Kunjungan berbeda yang diperiksa pemeriksa ini.</summary>
        public int VisitCount { get; set; }

        public int ExamCount { get; set; }

        /// <summary>Pemeriksaan yang direvisi sesudah final (versi lebih dari 1).</summary>
        public int RevisedExamCount { get; set; }
    }

    // ---------------------------------------------------------------- (2) penjualan

    /// <summary>
    /// Rekap penjualan dari baris tagihan Billing sumber <c>OPTICAL</c> yang aktif dan pertama kali ditagih pada periode.
    /// Nilai = jumlah × harga satuan baris tagihan (bruto). Diskon dan penyesuaian ada di tingkat tagihan Billing dan tidak
    /// dipotong di sini.
    /// </summary>
    public class OptSalesReportResponse
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public int LineCount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OptSalesByCategoryRow> ByCategory { get; set; } = new();
        public List<OptSalesByPaymentRow> ByPaymentType { get; set; } = new();
    }

    public class OptSalesByCategoryRow
    {
        /// <summary><c>EXAM</c> untuk pemeriksaan refraksi; selain itu nama enum <see cref="OptItemCategory"/>.</summary>
        public string CategoryCode { get; set; } = string.Empty;
        public string CategoryLabel { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
    }

    public class OptSalesByPaymentRow
    {
        public EncounterPaymentType PaymentType { get; set; }
        public string PaymentTypeLabel { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public decimal Amount { get; set; }
    }

    // ---------------------------------------------------------------- (3) pesanan terbuka

    public class OptOpenOrderReportResponse
    {
        public DateOnly AsOf { get; set; }

        /// <summary>Batas pengingat pengambilan dari pengaturan (hari).</summary>
        public int PickupReminderDays { get; set; }

        public int OpenOrderCount { get; set; }
        public int OverdueCount { get; set; }
        public int NeedsContactCount { get; set; }
        public List<OptOpenOrderRow> Orders { get; set; } = new();
    }

    public class OptOpenOrderRow
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public OptOrderKind OrderKind { get; set; }
        public OptOrderStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public string? WorkshopTypeLabel { get; set; }
        public string? ExternalLabSupplierName { get; set; }
        public DateOnly? EstimatedReadyDate { get; set; }
        public DateOnly? ReadyDate { get; set; }

        /// <summary>Masih dikerjakan padahal perkiraan jadi sudah lewat.</summary>
        public bool IsOverdue { get; set; }

        /// <summary>Hari keterlambatan dari perkiraan jadi.</summary>
        public int? DaysOverdue { get; set; }

        /// <summary>Siap diambil dan sudah melewati batas pengingat: perlu dihubungi.</summary>
        public bool NeedsContact { get; set; }

        public int? DaysSinceReady { get; set; }
    }

    // ---------------------------------------------------------------- (4) garansi

    public class OptWarrantyReportResponse
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public int ClaimCount { get; set; }
        public int WithinWarrantyCount { get; set; }
        public int OutOfWarrantyCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PendingCount { get; set; }
        public int RemakeCount { get; set; }
        public int RemakeHandedOverCount { get; set; }
        public List<OptWarrantyByTypeRow> ByComplaintType { get; set; } = new();
    }

    public class OptWarrantyByTypeRow
    {
        public OptComplaintType ComplaintType { get; set; }
        public string ComplaintTypeLabel { get; set; } = string.Empty;
        public int ClaimCount { get; set; }
        public int WithinWarrantyCount { get; set; }
        public int RemakeCount { get; set; }
    }
}
