using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    /// <summary>Pengajuan dispensasi oleh pegawai sendiri. Pegawai diambil dari sesi; penyetuju ditetapkan server.</summary>
    public class CreateMyDispensationRequest
    {
        /// <summary>Tanggal kerja. Paling lama MaxBackdatedCorrectionDays hari ke belakang (VR-PK-14); periode harus belum ditutup (VR-PK-15).</summary>
        public DateOnly AttendanceDate { get; set; }

        /// <summary>LateArrival (datang terlambat) atau EarlyDeparture (pulang lebih awal).</summary>
        [Required]
        [MaxLength(20)]
        public string DispensationType { get; set; } = string.Empty;

        /// <summary>Awal rentang yang dibebaskan. Nilai tanpa zona dibaca sebagai jam WIB.</summary>
        public DateTime StartAt { get; set; }

        /// <summary>Akhir rentang yang dibebaskan; harus sesudah StartAt (VR-PK-13).</summary>
        public DateTime EndAt { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>Kepala Unit mencatat dispensasi atas nama pegawai unitnya. Tidak ada ID penyetuju dari klien.</summary>
    public class CreateDispensationRequest : CreateMyDispensationRequest
    {
        public Guid WorkforceProfileId { get; set; }
    }

    public class DispensationDecisionRequest
    {
        /// <summary>Token konkurensi dari respons terakhir; berbeda = 409.</summary>
        public Guid RowVersion { get; set; }
    }

    public class RejectDispensationRequest : DispensationDecisionRequest
    {
        [Required]
        [MaxLength(500)]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class DispensationQueryRequest
    {
        /// <summary>Kosong = seluruh unit dalam cakupan baca. Unit di luar cakupan ditolak 403.</summary>
        public Guid? OrganizationUnitId { get; set; }

        /// <summary>Default awal bulan berjalan (WIB).</summary>
        public DateOnly? FromDate { get; set; }

        /// <summary>Default akhir bulan berjalan (WIB).</summary>
        public DateOnly? ToDate { get; set; }

        /// <summary>Submitted, Approved, Rejected, Cancelled.</summary>
        public string? Status { get; set; }

        /// <summary>true = hanya dispensasi Submitted yang penyetujunya pengguna ini (di unit mana pun).</summary>
        public bool AwaitingMyDecision { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class MyDispensationQueryRequest
    {
        /// <summary>Default awal bulan berjalan (WIB).</summary>
        public DateOnly? FromDate { get; set; }

        /// <summary>Default akhir bulan berjalan (WIB).</summary>
        public DateOnly? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DispensationResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? ProfileCode { get; set; }
        public string? EmployeeName { get; set; }

        /// <summary>Unit penempatan utama pegawai pada tanggal dispensasi.</summary>
        public Guid? OrganizationUnitId { get; set; }

        public string? OrganizationUnitName { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public string DispensationType { get; set; } = string.Empty;
        public string DispensationTypeLabel { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public bool IsOnBehalf { get; set; }
        public Guid RequestedByUserId { get; set; }
        public string? RequestedByName { get; set; }

        /// <summary>Atasan langsung yang ditetapkan server saat pengajuan.</summary>
        public Guid? ApproverWorkforceProfileId { get; set; }

        public string? ApproverName { get; set; }
        public Guid? DecidedByUserId { get; set; }
        public DateTime? DecidedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid RowVersion { get; set; }

        /// <summary>Pengguna saat ini adalah penyetuju dan dispensasi masih Submitted.</summary>
        public bool CanDecide { get; set; }

        /// <summary>Pengguna saat ini adalah pengaju dan dispensasi masih Submitted.</summary>
        public bool CanCancel { get; set; }
    }

    public class DispensationOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DispensationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public List<DispensationOptionResponse> StatusOptions { get; set; } = new();
        public List<DispensationOptionResponse> DispensationTypeOptions { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();

        /// <summary>Batas pengajuan mundur yang berlaku (MaxBackdatedCorrectionDays pada setelan kehadiran).</summary>
        public int MaxBackdatedDays { get; set; }
    }
}
