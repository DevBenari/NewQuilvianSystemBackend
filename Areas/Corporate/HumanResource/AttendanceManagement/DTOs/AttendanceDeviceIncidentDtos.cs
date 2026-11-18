using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    /// <summary>
    /// Satu kejadian gangguan mesin absen (HRD-DEC-202). Mencatat kejadian TIDAK membuat scan apa pun (INV-A52-07);
    /// pegawai terdampak tetap mengajukan koreksi per orang yang merujuk kejadian ini.
    /// </summary>
    public class CreateDeviceIncidentRequest
    {
        /// <summary>Mesin absen terdaftar (master mesin absen).</summary>
        public Guid AttendanceDeviceId { get; set; }

        /// <summary>Awal gangguan. Nilai tanpa zona dibaca sebagai jam WIB.</summary>
        public DateTime StartAt { get; set; }

        /// <summary>Akhir gangguan; harus sesudah StartAt (VR-PK-20).</summary>
        public DateTime EndAt { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }

    public class CancelDeviceIncidentRequest
    {
        /// <summary>Wajib (state 30.5).</summary>
        [MaxLength(500)]
        public string? CancelReason { get; set; }
    }

    public class DeviceIncidentQueryRequest
    {
        /// <summary>Riwayat gangguan per mesin (HRD-DEC-202 butir 6).</summary>
        public Guid? AttendanceDeviceId { get; set; }

        /// <summary>Kejadian yang bersinggungan dengan rentang tanggal (WIB) ini.</summary>
        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }

        /// <summary>Active atau Cancelled.</summary>
        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DeviceIncidentResponse
    {
        public Guid Id { get; set; }
        public Guid AttendanceDeviceId { get; set; }
        public string? AttendanceDeviceCode { get; set; }
        public string? AttendanceDeviceName { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public Guid ReportedByUserId { get; set; }
        public string? ReportedByName { get; set; }
        public string? CancelReason { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>Jumlah koreksi presensi yang merujuk kejadian ini.</summary>
        public int ReferencedCorrectionCount { get; set; }
    }

    /// <summary>Pilihan kejadian gangguan Active pada formulir koreksi.</summary>
    public class DeviceIncidentOptionResponse
    {
        public Guid Value { get; set; }

        /// <summary>Contoh: "IGD-01 Mesin IGD — 03-02-2027 06.30–09.00 — Mesin mati listrik".</summary>
        public string Label { get; set; } = string.Empty;

        public Guid AttendanceDeviceId { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
    }
}
