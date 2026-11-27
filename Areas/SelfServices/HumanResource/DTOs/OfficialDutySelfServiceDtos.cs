using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs
{
    /// <summary>
    /// Saringan daftar surat tugas dinas luar milik sendiri (kontrak v31 API 37.5). Tidak ada parameter pegawai: identitas dari sesi.
    /// </summary>
    public class OfficialDutySelfQuery
    {
        /// <summary>Surat tugas yang berakhir pada atau sesudah tanggal ini.</summary>
        public DateOnly? FromDate { get; set; }

        /// <summary>Surat tugas yang mulai pada atau sebelum tanggal ini.</summary>
        public DateOnly? ToDate { get; set; }

        /// <summary>Submitted, Approved, Rejected, atau Cancelled. Kosong = semua.</summary>
        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;

        /// <summary>Paling banyak 100.</summary>
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Pengajuan surat tugas dinas luar dari Layanan Mandiri. Tanpa WorkforceProfileId: pegawai dari sesi (VR-PK-34).
    /// </summary>
    public class CreateOfficialDutySelfRequest
    {
        /// <summary>Salah satu nilai <c>dutyCategoryOptions</c> pada metadata.</summary>
        [Required, MaxLength(50)]
        public string DutyCategory { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string AssignmentOrderNumber { get; set; } = string.Empty;

        [Required]
        public DateTime? StartAt { get; set; }

        /// <summary>Harus sesudah <c>startAt</c>.</summary>
        [Required]
        public DateTime? EndAt { get; set; }

        [Required, MaxLength(255)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public string Purpose { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        /// <summary>Kunci hasil <c>POST attachments</c> oleh akun ini sendiri; kosong bila tanpa lampiran.</summary>
        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }
    }

    public class OfficialDutySelfFilterMetadataResponse
    {
        public List<OfficialDutySelfOptionResponse> DutyCategoryOptions { get; set; } = new();
        public List<OfficialDutySelfOptionResponse> StatusOptions { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class OfficialDutySelfOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>Surat tugas dinas luar milik sendiri; tanpa data pegawai lain (penyetuju tidak ditampilkan).</summary>
    public class OfficialDutySelfResponse
    {
        public Guid Id { get; set; }
        public string DutyCategory { get; set; } = string.Empty;
        public string AssignmentOrderNumber { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string Destination { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? CancelDateTime { get; set; }

        public static OfficialDutySelfResponse From(OfficialDutyResponse x) => new()
        {
            Id = x.Id,
            DutyCategory = x.DutyCategory,
            AssignmentOrderNumber = x.AssignmentOrderNumber,
            StartAt = x.StartAt,
            EndAt = x.EndAt,
            Destination = x.Destination,
            Purpose = x.Purpose,
            AttachmentUrl = x.AttachmentUrl,
            Status = x.Status,
            StatusLabel = OfficialDutySelfStatus.Label(x.Status),
            RejectionReason = x.RejectionReason,
            CreateDateTime = x.CreateDateTime,
            CancelDateTime = x.CancelDateTime
        };
    }

    public static class OfficialDutySelfStatus
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

    public static class OfficialDutySelfCategory
    {
        // Nilai dari AttendanceValueConstants.OfficialDutyCategory (yang ditegakkan service); label mengikuti layar HR.
        public static readonly (string Value, string Label)[] All =
        {
            (AttendanceValueConstants.OfficialDutyCategory.AmbulanceReferral, "Pengawalan Ambulans Rujukan"),
            (AttendanceValueConstants.OfficialDutyCategory.LabCourier, "Kurir Spesimen & Laboratorium"),
            (AttendanceValueConstants.OfficialDutyCategory.TrainingSeminar, "Pelatihan, Seminar & Workshop"),
            (AttendanceValueConstants.OfficialDutyCategory.HomeCare, "Pelayanan Pasien Home Care"),
            (AttendanceValueConstants.OfficialDutyCategory.Other, "Tugas Resmi Lainnya")
        };
    }
}
