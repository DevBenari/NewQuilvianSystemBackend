using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs
{
    /// <summary>
    /// Saringan daftar izin pulang cepat milik sendiri (kontrak v30 API 36.4). Tidak ada parameter pegawai: identitas dari sesi.
    /// </summary>
    public class EarlyLeaveSelfQuery
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }

        /// <summary>Submitted, Approved, Rejected, atau Cancelled. Kosong = semua.</summary>
        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;

        /// <summary>Paling banyak 100.</summary>
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Pengajuan izin pulang cepat dari Layanan Mandiri. Tanpa WorkforceProfileId/AttendanceDailyId: pegawai dari sesi,
    /// rekap harian dicari dari AttendanceDate (VR-PK-32, VR-PK-34).
    /// </summary>
    public class CreateEarlyLeaveSelfRequest
    {
        [Required]
        public DateOnly? AttendanceDate { get; set; }

        /// <summary>Jam rencana pulang; tidak boleh lebih awal dari jam mulai kerja yang dijadwalkan (aturan yang ada).</summary>
        [Required]
        public DateTime? RequestedEarlyLeaveAt { get; set; }

        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>Pembatalan pengajuan Layanan Mandiri yang masih Submitted (VR-PK-33). Dipakai izin pulang cepat dan dinas luar.</summary>
    public class CancelSelfRequest
    {
        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    public class EarlyLeaveSelfFilterMetadataResponse
    {
        public List<EarlyLeaveSelfOptionResponse> StatusOptions { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class EarlyLeaveSelfOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>Izin pulang cepat milik sendiri; tanpa data pegawai lain (penyetuju tidak ditampilkan).</summary>
    public class EarlyLeaveSelfResponse
    {
        public Guid Id { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public DateTime RequestedEarlyLeaveAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? CancelDateTime { get; set; }

        public static EarlyLeaveSelfResponse From(EarlyLeavePermissionResponse x) => new()
        {
            Id = x.Id,
            AttendanceDate = x.AttendanceDate,
            RequestedEarlyLeaveAt = x.RequestedEarlyLeaveAt,
            Reason = x.Reason,
            Status = x.Status,
            StatusLabel = EarlyLeaveSelfStatus.Label(x.Status),
            RejectionReason = x.RejectionReason,
            CreateDateTime = x.CreateDateTime,
            CancelDateTime = x.CancelDateTime
        };
    }

    public static class EarlyLeaveSelfStatus
    {
        public static readonly (string Value, string Label)[] All =
        {
            ("Submitted", "Diajukan"),
            ("Approved", "Disetujui"),
            ("Rejected", "Ditolak"),
            ("Cancelled", "Dibatalkan")
        };

        public static string Label(string status) =>
            All.FirstOrDefault(x => string.Equals(x.Value, status, StringComparison.OrdinalIgnoreCase)).Label ?? status;
    }
}
