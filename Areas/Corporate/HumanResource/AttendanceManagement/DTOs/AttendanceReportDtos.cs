namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    public class AttendanceReportQueryRequest
    {
        /// <summary>Kosong = seluruh unit dalam cakupan baca pengguna. Unit di luar cakupan ditolak 403 (VR-PK-27).</summary>
        public Guid? OrganizationUnitId { get; set; }

        /// <summary>Default awal bulan berjalan (WIB). Rentang paling lama 92 hari (VR-PK-28).</summary>
        public DateOnly? FromDate { get; set; }

        /// <summary>Default akhir bulan berjalan (WIB).</summary>
        public DateOnly? ToDate { get; set; }

        /// <summary>
        /// Saring status sesuai laporan: shift = status shift; attendance/lateness = status harian;
        /// absence = kategori (Absent, NoScan, IncompletePunch, Sick, Leave); leave = status cuti; overtime = status lembur.
        /// Tidak dipakai pada unit-summary.
        /// </summary>
        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AttendanceReportColumnResponse
    {
        /// <summary>Nama properti pada AttendanceReportRow (camelCase), dipakai layar dan ekspor.</summary>
        public string Key { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class AttendanceReportResponse
    {
        public string ReportCode { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public Guid? OrganizationUnitId { get; set; }

        /// <summary>
        /// Closed bila setiap tanggal dalam rentang sudah tercakup periode kehadiran yang ditutup (data final);
        /// Open bila masih ada tanggal pada periode terbuka atau belum berperiode (data dapat berubah, HRD-DEC-206 butir 6).
        /// </summary>
        public string PeriodState { get; set; } = "Open";

        /// <summary>Kolom yang ditampilkan untuk laporan ini, berurutan.</summary>
        public List<AttendanceReportColumnResponse> Columns { get; set; } = new();

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<AttendanceReportRow> Items { get; set; } = new();
    }

    /// <summary>
    /// Satu baris laporan. Hanya kolom yang tercantum pada Columns yang terisi untuk laporan tersebut.
    /// Lampiran, surat dokter, diagnosis, dan alasan cuti/sakit tidak pernah ada di sini (HRD-DEC-206 butir 5).
    /// </summary>
    public class AttendanceReportRow
    {
        public Guid? WorkforceProfileId { get; set; }
        public string? ProfileCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }

        public DateOnly? Date { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? ShiftName { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public string? RosterPeriodCode { get; set; }

        public string? Status { get; set; }
        public string? StatusLabel { get; set; }

        /// <summary>Jenis cuti atau keterangan ketidakhadiran. Cuti sakit selalu hanya "Sakit".</summary>
        public string? Category { get; set; }

        public decimal? Days { get; set; }
        public int? LateMinutes { get; set; }
        public int? EarlyLeaveMinutes { get; set; }
        public int? WorkMinutes { get; set; }
        public int? RequestedMinutes { get; set; }
        public int? ApprovedMinutes { get; set; }

        public int? EmployeeCount { get; set; }
        public int? PresentDays { get; set; }
        public int? LateCount { get; set; }
        public int? AbsentDays { get; set; }
        public int? NoScanDays { get; set; }
        public int? IncompleteDays { get; set; }
        public int? LeaveDays { get; set; }
        public int? SickDays { get; set; }
        public int? OvertimeMinutes { get; set; }
    }
}
