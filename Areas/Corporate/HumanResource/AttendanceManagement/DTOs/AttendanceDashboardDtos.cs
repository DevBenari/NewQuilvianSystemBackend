using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    public class AttendanceDashboardQueryRequest
    {
        /// <summary>Tanggal kerja (WIB). Kosong = hari ini.</summary>
        public DateOnly? Date { get; set; }

        /// <summary>Kosong = seluruh unit dalam cakupan baca pengguna. Unit di luar cakupan ditolak 403 (VR-PK-27).</summary>
        public Guid? OrganizationUnitId { get; set; }
    }

    public class AttendanceDashboardDetailQueryRequest : AttendanceDashboardQueryRequest
    {
        /// <summary>Angka yang dirinci (Metric pada kartu). Kosong = angka utama kartu.</summary>
        public string? Metric { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AttendanceDashboardResponse
    {
        public DateOnly Date { get; set; }
        public Guid? OrganizationUnitId { get; set; }

        /// <summary>Jam hitung (UTC). Angka dihitung saat diminta, tidak diperbarui sendiri (HRD-DEC-208 butir 2).</summary>
        public DateTime CalculatedAt { get; set; }

        /// <summary>
        /// Saat yang dipakai kartu Pegawai Bertugas (UTC): jam hitung bila tanggal = hari ini, selain itu jam yang sama
        /// pada tanggal yang dipilih.
        /// </summary>
        public DateTime OnDutyReferenceAt { get; set; }

        public List<AttendanceDashboardCardResponse> Cards { get; set; } = new();
    }

    public class AttendanceDashboardCardResponse
    {
        /// <summary>attendance-today, on-duty, lateness, absence, incomplete-schedule.</summary>
        public string CardCode { get; set; } = string.Empty;

        public string CardName { get; set; } = string.Empty;

        /// <summary>Angka utama kartu = Count pada Metrics yang pertama.</summary>
        public int Count { get; set; }

        /// <summary>Setiap angka sama dengan TotalData rincian untuk Metric yang sama (FR-PK-031).</summary>
        public List<AttendanceDashboardMetricResponse> Metrics { get; set; } = new();
    }

    public class AttendanceDashboardMetricResponse
    {
        public string Metric { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>Rincian satu angka kartu, berpaginasi; TotalData sama dengan angka pada kartu.</summary>
    public class AttendanceDashboardDetailResponse : PagedResult<DashboardDetailRow>
    {
        public string CardCode { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public string Metric { get; set; } = string.Empty;
        public string MetricLabel { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public DateTime CalculatedAt { get; set; }
    }

    /// <summary>
    /// Satu baris di balik angka dashboard. Hanya kolom yang relevan bagi kartunya yang terisi.
    /// Alasan cuti, lampiran, surat dokter, dan diagnosis tidak pernah ada di sini; sakit hanya "Sakit" (HRD-DEC-208 butir 5).
    /// </summary>
    public class DashboardDetailRow
    {
        public Guid? WorkforceProfileId { get; set; }
        public string? ProfileCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }

        public DateOnly? Date { get; set; }

        /// <summary>Kategori tampilan, mis. "Hadir", "Terlambat", "Sakit", "Shift", "Petugas Pengganti", "Siaga Utama".</summary>
        public string? Category { get; set; }

        public string? ShiftName { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public int? LateMinutes { get; set; }

        /// <summary>Status harian, status roster, atau status penugasan siaga (nilai mentah).</summary>
        public string? Status { get; set; }

        /// <summary>Kartu Jadwal Belum Lengkap: kode roster periode.</summary>
        public string? RosterPeriodCode { get; set; }

        /// <summary>Kartu Jadwal Belum Lengkap: W6 (kuota) atau W7 (kualifikasi).</summary>
        public string? RuleCode { get; set; }

        /// <summary>Kartu Jadwal Belum Lengkap: pesan peringatan pemeriksaan roster.</summary>
        public string? Message { get; set; }
    }
}
