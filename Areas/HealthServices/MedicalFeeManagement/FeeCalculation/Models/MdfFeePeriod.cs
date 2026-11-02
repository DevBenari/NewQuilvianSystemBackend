using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models
{
    /// <summary>Status siklus periode perhitungan jasa medis — data-dictionary.md bagian 4.</summary>
    public static class MdfFeePeriodStatuses
    {
        public const string Open = "Open";
        public const string Calculated = "Calculated";
        public const string Verified = "Verified";
        public const string Approved = "Approved";
        public const string Closed = "Closed";
    }

    /// <summary>
    /// Periode perhitungan jasa medis (aggregate root) — MDF-DES-010, MDF-DES-012, MDF-DES-016.
    /// Mengelompokkan seluruh hasil jasa satu bulan dan mengunci angkanya saat status Closed.
    /// Periode tidak dapat ditutup bila masih ada layanan unresolved berstatus Open (MDF-DES-016).
    /// </summary>
    [Table("MdfFeePeriod", Schema = "public")]
    public class MdfFeePeriod : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kode periode unik, contoh: '2026-09' atau '2026-10'.</summary>
        [Required]
        [MaxLength(20)]
        public string PeriodCode { get; set; } = string.Empty;

        /// <summary>Tanggal awal cakupan periode layanan.</summary>
        [Required]
        public DateOnly PeriodStart { get; set; }

        /// <summary>Tanggal akhir cakupan periode layanan (wajib >= PeriodStart).</summary>
        [Required]
        public DateOnly PeriodEnd { get; set; }

        /// <summary>Status siklus: Open, Calculated, Verified, Approved, Closed (MdfFeePeriodStatuses).</summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = MdfFeePeriodStatuses.Open;

        /// <summary>Waktu kalkulasi terakhir dijalankan.</summary>
        public DateTime? CalculatedAt { get; set; }

        /// <summary>User ID yang menjalankan kalkulasi terakhir.</summary>
        public Guid? CalculatedBy { get; set; }

        /// <summary>User ID pemverifikasi hasil periode (wajib berbeda dari ApprovedBy).</summary>
        public Guid? VerifiedBy { get; set; }

        /// <summary>Waktu verifikasi periode diselesaikan.</summary>
        public DateTime? VerifiedAt { get; set; }

        /// <summary>User ID penyetujui periode (wajib berbeda dari VerifiedBy).</summary>
        public Guid? ApprovedBy { get; set; }

        /// <summary>Waktu persetujuan periode diberikan.</summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>User ID penutup periode.</summary>
        public Guid? ClosedBy { get; set; }

        /// <summary>Waktu periode ditutup dan dikunci.</summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>Token konkurensi optimistik (MDF-DES-005).</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public ICollection<MdfServiceFee> ServiceFees { get; set; } = new List<MdfServiceFee>();
        public ICollection<MdfUnresolvedService> UnresolvedServices { get; set; } = new List<MdfUnresolvedService>();
    }
}

