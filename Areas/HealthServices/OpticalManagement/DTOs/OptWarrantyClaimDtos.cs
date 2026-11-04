using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs
{
    // OPT-API-v1 grup "Health Services / Optical Management / Opt Warranty Claim" (BE-OPT-050).

    /// <summary>Catat keluhan garansi pada kunjungan keluhan yang baru didaftarkan (<c>OPT-DEC-043</c>).</summary>
    public class OptWarrantyClaimCreateRequest
    {
        /// <summary>Pesanan asal yang sudah diserahkan.</summary>
        public Guid? OrderId { get; set; }

        /// <summary>Kunjungan Optik baru milik pasien yang sama, bukan kunjungan pesanan asal (<c>OPT-VAL-061</c>).</summary>
        public Guid? EncounterId { get; set; }

        public OptComplaintType? ComplaintType { get; set; }

        public string? ComplaintNote { get; set; }
    }

    /// <summary>Hasil pemeriksaan ulang oleh RO/Sp.M.</summary>
    public class OptWarrantyClaimExamineRequest
    {
        public string? ExaminationFinding { get; set; }

        /// <summary>Pemeriksaan refraksi ulang pada kunjungan keluhan, bila dilakukan.</summary>
        public Guid? ReexamRefractionExamId { get; set; }

        public int? ExpectedVersion { get; set; }
    }

    /// <summary>Keputusan Kepala Optik.</summary>
    public class OptWarrantyClaimDecisionRequest
    {
        public bool Approve { get; set; }

        /// <summary>Wajib bila menolak.</summary>
        public string? DecisionReason { get; set; }

        /// <summary>Baris pesanan asal yang dibuat ulang. Kosong = seluruh baris aktif pesanan asal.</summary>
        public List<Guid>? RemakeItems { get; set; }

        /// <summary>
        /// Versi resep untuk remake, mis. resep baru hasil pemeriksaan ulang. Kosong = versi resep pesanan asal. Tambahan atas
        /// kontrak (<c>BE-OPT-050</c>): keluhan ukuran sering berakhir dengan resep baru.
        /// </summary>
        public Guid? PrescriptionVersionId { get; set; }

        public int? ExpectedVersion { get; set; }
    }

    public class OptWarrantyClaimListQuery
    {
        public OptWarrantyClaimStatus? Status { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? PatientId { get; set; }

        /// <summary>Mencari nomor keluhan, nomor pesanan, nomor RM, atau nama pasien.</summary>
        public string? Search { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class OptWarrantyClaimResponse
    {
        public Guid Id { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid EncounterId { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public OptComplaintType ComplaintType { get; set; }
        public string ComplaintTypeLabel { get; set; } = string.Empty;
        public string ComplaintNote { get; set; } = string.Empty;

        /// <summary>Dihitung saat dicatat dari masa garansi yang dibekukan pada penyerahan (<c>OPT-DEC-041</c>).</summary>
        public bool IsWithinWarranty { get; set; }

        /// <summary>Batas akhir garansi untuk jenis keluhan ini (tanggal WIB); kosong untuk kerusakan pemakaian.</summary>
        public DateOnly? WarrantyDeadlineSnapshot { get; set; }

        public OptWarrantyClaimStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid? ExaminedByWorkforceProfileId { get; set; }
        public DateTime? ExaminedAt { get; set; }
        public string? ExaminationFinding { get; set; }
        public Guid? ReexamRefractionExamId { get; set; }
        public DateTime? DecidedAt { get; set; }
        public string? DecisionReason { get; set; }
        public Guid? RemakeOrderId { get; set; }
        public string? RemakeOrderNumber { get; set; }
        public int Version { get; set; }
    }
}
